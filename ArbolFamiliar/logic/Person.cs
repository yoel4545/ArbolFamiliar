using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ArbolFamiliar
{
    public class Person
    {
        public string name;
        public string id;
        public DateTime birthdate;
        public DateTime? deathDate;
        public string fotoPath;
        public List<Person> children;
        public Person[] parents;
        public float x;  //Coordenada x para graficar
        public float y; //Coordenada y para graficar
        public Person partner;
        public int level;
        public double Latitud;
        public double Longitud;

        public int Edad
        {
            get
            {
                DateTime fechaReferencia = deathDate ?? DateTime.Now;
                int edad = fechaReferencia.Year - birthdate.Year;

                if (birthdate.Date > fechaReferencia.AddYears(-edad))
                    edad--;

                return edad;
            }
        }

        public Person(string name, string id, DateTime birthdate, string photoPath, double lat = 0, double lng = 0, DateTime? deathDate = null) //Metodo constructor con informacion basica, para un familiar vivo
        {
            this.name = name;
            this.id = id;

            this.birthdate = birthdate;
            this.deathDate = deathDate;
            this.fotoPath = photoPath;
            children = new List<Person>();
            parents = new Person[2]; //Maximo dos padres
            this.Latitud = lat;
            this.Longitud = lng;
            this.level = 0;

        }

        public Person(string name, string id, DateTime birthdate, string photoPath)
            : this(name, id, birthdate, photoPath, 0, 0, null)
        {
        }

        public void SetLevel(int newLevel)
        {
            level = newLevel;
        }

        public bool HasSiblings()
        {
            // Si no tiene arreglo de padres o ninguno asignado, no puede tener hermanos
            if (parents == null) return false;

            for (int i = 0; i < parents.Length; i++)
            {
                var p = parents[i];
                if (p == null) continue;
                if (p.children != null)
                {
                    int otherChildren = 0;
                    foreach (var c in p.children)
                    {
                        if (c == null) continue;
                        if (c != this) otherChildren++;
                        if (otherChildren > 0) return true; // hay al menos otro hijo -> hermano
                    }
                }
            }
            return false;
        }

        public void AddChild(Person child) //Añade un hijo a la lista de hijos propia
        {
            if (children == null)
            {
                children = new List<Person>();
            }
            if (child != null && !children.Contains(child))
            {
                children.Add(child);

                // ✅ ACTUALIZADO: Actualizar nivel del hijo y propagar a sus descendientes
                ActualizarNivelYPropagar(child, this.level + 1);

                // ✅ Si tengo pareja, también agregar el hijo a la pareja
                if (partner != null && !partner.children.Contains(child))
                {
                    partner.children.Add(child);
                }
            }
        }

        public float GetRightMostChildX()
        {
            float rightMostX = float.MinValue;
            foreach (var child in children)
            {
                if (child.x > rightMostX)
                {
                    rightMostX = child.x;
                }
            }
            return rightMostX;
        }
        private void ActualizarNivelYPropagar(Person persona, int nuevoNivel)
        {
            if (persona.level == nuevoNivel) return;

            persona.level = nuevoNivel;

            // Propagar a todos los hijos
            foreach (var hijo in persona.children)
            {
                ActualizarNivelYPropagar(hijo, nuevoNivel + 1);
            }

            // ✅ ACTUALIZAR también a la pareja para mantener consistencia
            if (persona.partner != null && persona.partner.level != nuevoNivel)
            {
                persona.partner.level = nuevoNivel;
            }
        }
        public bool CanAddPartner(Person newPartner)
        {
            // No puede tener ya pareja
            if (partner != null) return false;

            // La nueva persona tampoco debe tener pareja
            if (newPartner.partner != null) return false;

            // Evitar asignarse a sí mismo
            if (newPartner == this) return false;

            return true;
        }

        public void AddPartner(Person newPartner)
        {

            if (!CanAddPartner(newPartner)) return;

            partner = newPartner;
            newPartner.partner = this;


            // Asegurar que ambas tengan listas inicializadas
            if (newPartner.children == null)
                newPartner.children = new List<Person>();

            // Copiar hijos de this a newPartner (sin duplicados)
            foreach (var hijo in this.children)
            {
                if (!newPartner.children.Contains(hijo))
                {
                    newPartner.children.Add(hijo);
                    newPartner.AddPartnerToChild(hijo);
                }
            }

            // Copiar hijos de newPartner a this (sin duplicados)
            foreach (var hijo in newPartner.children)
            {
                if (!this.children.Contains(hijo))
                {
                    this.children.Add(hijo);
                }
            }

            // Mantener niveles coherentes
            newPartner.level = this.level;
        }

        public void AddPartnerToChild(Person child)
        {
            if (child == null) return;
            if (child.Parents == null) return;

            if (child.Parents[0] == this || child.Parents[1] == this)
                return;

            if (child.Parents[0] == null)
                child.Parents[0] = this;
            else if (child.Parents[1] == null)
                child.Parents[1] = this;
        }

        public bool HasPartner()
        {
            if (partner != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool CanAddParent()
        {
            // Si ya tiene dos padres, no puede agregar más
            if (parents != null && parents.Length >= 2 && parents[0] != null && parents[1] != null)
                return false;

            // Si tiene pareja y la pareja ya tiene padres,
            // permitir sólo si la pareja NO tiene hermanos.
            if (partner != null)
            {
                bool partnerHasAnyParent = (partner.parents != null) &&
                                           (partner.parents.Length > 0) &&
                                           (partner.parents[0] != null || (partner.parents.Length > 1 && partner.parents[1] != null));

                if (partnerHasAnyParent)
                {
                    // Si la pareja tiene hermanos -> no permitir
                    if (partner.HasSiblings())
                        return false;
                    // Si la pareja NO tiene hermanos -> permitir (seguir)
                }
            }

            // En cualquier otro caso, permitir si aún no tiene 2 padres
            return true;
        }

        public void AddParent(Person parent)
        {
            if (parent == null && CanAddParent()) return;

            // Si ya tiene 2 padres → no agregar más
            if (parents[0] != null && parents[1] != null)
                return;

            // Añadir padre en el espacio vacío
            if (parents[0] == null)
                parents[0] = parent;
            else if (parents[1] == null)
                parents[1] = parent;

            // Asegurar que el padre tenga lista de hijos
            if (parent.children == null)
                parent.children = new List<Person>();

            // Agregar hijo al padre
            if (!parent.children.Contains(this))
            {
                parent.children.Add(this);
                // ✅ ACTUALIZADO: Usar el nuevo método para propagar niveles
                parent.ActualizarNivelYPropagar(this, parent.level + 1);
            }

            // Si el padre tiene pareja, esa pareja también es padre
            if (parent.partner != null)
            {
                var otro = parent.partner;

                // agregar hijo al partner
                if (!otro.children.Contains(this))
                {
                    otro.children.Add(this);
                }

                // agregar como padre si hay espacio y no está ya
                if (parents[0] != otro && parents[1] != otro)
                {
                    if (parents[0] == null) parents[0] = otro;
                    else if (parents[1] == null) parents[1] = otro;
                }
            }

            // Si ya hay ambos padres, asegurarse que sean pareja
            if (parents[0] != null && parents[1] != null && parents[0].partner != parents[1])
            {
                parents[0].AddPartner(parents[1]);
            }
        }

        public int CountParents() //Cuenta la cantidad de padres que tiene la persona
        {
            int count = 0;
            if (parents[0] != null) count++;
            if (parents[1] != null) count++;
            return count;
        }


        public void RemoveChild(Person child) //Elimina un hijo de la lista de hijos propia
        {
            if (child == null) return;
            if (children.Contains(child))
            {
                children.Remove(child);
            }
        }

        public void RemoveParent(Person parent) //Elimina un padre de la lista de padres propia
        {
            if (parent == null) return;
            if (parents[0] == parent)
            {
                parents[0] = null;
            }
            else if (parents[1] == parent)
            {
                parents[1] = null;
            }
        }
        public bool FechasValidas()
        {
            if (birthdate > DateTime.Now)
                return false;

            if (deathDate.HasValue && deathDate.Value <= birthdate)
                return false;

            return true;
        }

        public Person[] Parents => parents;
        public List<Person> Children => children;
        public int GetLevel => level;

        public Person Partner => partner;

        public string GetName
        {
            get => name;
        }

        public string GetId => id;

        public float GetX => x;
        public float GetY => y;

        public float RightMostParent()
        {
            if (parents[0] != null)
            {
                if (parents[1] != null)
                {
                    Debug.WriteLine("Dos padres");
                    return Math.Max(parents[0].x, parents[1].x);
                }
                else
                {
                    Debug.WriteLine("Un solo padre");
                    return parents[0].x;
                }
            }
            
            return x;
        }


        public string ToOwnPropertiesWithRelativesString()
        {
            // Propiedades propias
            var death = deathDate.HasValue ? deathDate.Value.ToString("yyyy-MM-dd") : "N/A";
            var foto = string.IsNullOrEmpty(fotoPath) ? "N/A" : fotoPath;

            // Pareja (solo nombre)
            string pareja = partner != null ? partner.GetName : "Ninguna";

            // Hijos (solo nombres, separados por coma)
            string hijos;
            if (children == null || children.Count == 0)
            {
                hijos = "Ninguno";
            }
            else
            {
                hijos = string.Join(", ", children.Where(h => h != null).Select(h => h.GetName));
            }

            return
                $"Nombre: {name ?? ""}\n" +
                $"Level: {level.ToString() ?? ""}\n" +
                $"Pareja: {pareja}\n" +
                $"Hijos: {hijos}\n";
        }

        public void setX(int x)
        {
            this.x = x;
        }

        public void setY(int y)
        {
            this.y = y;
        }

        public void PrintOwnPropertiesWithRelatives()
        {
            Debug.WriteLine(ToOwnPropertiesWithRelativesString());
        }
    }
}