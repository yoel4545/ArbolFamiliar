using System;
using System.Collections.Generic;
using System.Linq;

namespace ArbolFamiliar
{
    public class Person
    {
        public string name { get; set; }
        public string id { get; set; }
        private DateTime birthdate { get; set; }
        private DateTime? deathDate { get; set; }
        public string fotoPath { get; set; }
        public List<Person> children { get; set; }
        public Person[] parents { get; set; }
        public int x { get; set; } //Coordenada x para graficar
        public int y { get; set; } //Coordenada y para graficar
        public Person partner { get; set; }
        private int level { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }


        public Person(string name, string id, DateTime birthdate, string photoPath, double lat, double lng) //Metodo constructor con informacion basica, para un familiar vivo
        {
            this.name = name;
            this.id = id;
            this.birthdate = birthdate;
            deathDate = null;
            this.fotoPath = photoPath;
            children = new List<Person>();
            parents = new Person[2]; //Maximo dos padres
            this.Latitud = lat;    // ← NUEVO
            this.Longitud = lng;
        }

        public Person(string name, string id, DateTime birthdate, string photoPath)
        {
            this.name = name;
            this.id = id;
            this.birthdate = birthdate;
            deathDate = null;
            this.fotoPath = photoPath;
            children = new List<Person>();
            parents = new Person[2];
        }

        public void SetLevel(int newLevel)
        {
            level = newLevel;
        }

        public void AddChild(Person child) //Anade un hijo a la lista de hijos propia
        {
            if (children == null)
            {
                return;
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
            if (!CanAddPartner(newPartner)) return; // protección

            partner = newPartner;
            newPartner.partner = this;

            newPartner.SetLevel(level);
            newPartner.children = this.children; // comparten lista de hijos
        }

        public void AddChildList(Person existingPartner)
        {
            children = existingPartner.Children;
        }

        public bool CanAddParent()
        {
            if (parents[0] != null && parents[1] != null) return false;

            // Si tengo pareja y la pareja ya tiene algún padre -> no permitir
            if (partner != null)
            {
                if ((partner.parents[0] != null) || (partner.parents[1] != null))
                    return false;
            }

            return true;
        }

        public void AddParent(Person parent)
        {
            if (parent == null) return;
            if (!CanAddParent()) return;

            // asignar en el primer slot libre
            if (parents[0] == null)
            {
                parents[0] = parent;
            }
            else if (parents[1] == null)
            {
                parents[1] = parent;
            }

            if (!parent.children.Contains(this))
            {
                parent.children.Add(this);
            }

            // Si el padre ya tiene pareja, ese partner también debe tener este hijo
            if (parent.partner != null)
            {
                if (parent.partner.children == null) parent.partner.children = parent.children;
                else if (!parent.partner.children.Contains(this))
                {
                    parent.partner.children.Add(this);
                }
            }

            // Si ahora tengo ambos padres, asegurar que los padres sean pareja entre sí
            if (parents[0] != null && parents[1] != null)
            {
                // establecer pareja mutua entre los padres
                if (parents[0].partner != parents[1])
                {
                    parents[0].AddPartner(parents[1]);
                }

                if (parents[1].partner != parents[0])
                {
                    parents[1].AddPartner(parents[0]);
                }
                    
            }

            parent.SetLevel(this.level - 1);
            if (parent.partner != null)
                parent.partner.SetLevel(parent.GetLevel); // pareja al mismo nivel
        }

        public void RemoveChild(Person child) //Elimina un hijo de la lista de hijos propia
        {
            if (child != null) return;
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
