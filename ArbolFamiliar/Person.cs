using System;
using System.Collections.Generic;
using System.Linq;

namespace ArbolFamiliar
{
    public class Person
    {
        public string name { get; set; }
        public string id { get; set; }
        public DateTime birthdate { get; set; }
        public DateTime? deathDate { get; set; }
        public string fotoPath { get; set; }
        public List<Person> children { get; set; }
        public Person[] parents { get; set; }
        public float x { get; set; } //Coordenada x para graficar
        public float y { get; set; } //Coordenada y para graficar
        public Person partner { get; set; }
        public int level { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }

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

        public Person(string name, string id, DateTime birthdate, string photoPath, double lat=0, double lng = 0, DateTime? deathDate = null) //Metodo constructor con informacion basica, para un familiar vivo
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

        public void AddChild(Person child) //Anade un hijo a la lista de hijos propia
        {
            if (children == null)
            {
                children = new List<Person>();
            }
            if (child != null && !children.Contains(child))
            {
                children.Add(child);
                child.SetLevel(level + 1);
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




        public bool CanAddParent()
        {
            // Solo evitar agregar más de dos padres
            return !(parents[0] != null && parents[1] != null);
        }

        public void AddParent(Person parent)
        {
            if (parent == null) return;

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
                parent.children.Add(this);

            // Si el padre tiene pareja, esa pareja también es padre
            if (parent.partner != null)
            {
                var otro = parent.partner;

                // agregar hijo al partner
                if (!otro.children.Contains(this))
                    otro.children.Add(this);

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

            // Ajustar niveles
            parent.level = Math.Max(0, this.level - 1);
            if (parent.partner != null)
                parent.partner.level = parent.level;
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
    }
}
