using System;
using System.Collections.Generic;
using System.Linq;

namespace ArbolFamiliar
{
    public class Person
    {
        // PROPIEDADES BÁSICAS
        public string name { get; set; }
        public string id { get; set; }
        public DateTime birthdate { get; set; }
        public DateTime? deathDate { get; set; }
        public string fotoPath { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public bool EsSubArbol { get; set; }
        public Person RaizSubArbol { get; set; }

        // RELACIONES FAMILIARES 

        public List<Person> children { get; set; } = new List<Person>();
        public Person[] parents { get; set; } = new Person[2];
        public Person partner { get; set; }

        // DATOS DE VISUALIZACIÓN
        public float x { get; set; }
        public float y { get; set; }
        public int level { get; set; }

        // PROPIEDADES DERIVADAS

        public int Edad
        {
            get
            {
                DateTime fechaReferencia = deathDate ?? DateTime.Now;
                int edad = fechaReferencia.Year - birthdate.Year;
                if (birthdate.Date > fechaReferencia.AddYears(-edad)) edad--;
                return edad;
            }
        }

        // CONSTRUCTORES 

        public Person(string name, string id, DateTime birthdate, string photoPath, double lat = 0, double lng = 0, DateTime? deathDate = null)
        {
            this.name = name;
            this.id = id;
            this.birthdate = birthdate;
            this.deathDate = deathDate;
            this.fotoPath = photoPath;
            this.Latitud = lat;
            this.Longitud = lng;
            this.level = 0;
        }

        public Person(string name, string id, DateTime birthdate, string photoPath)
            : this(name, id, birthdate, photoPath, 0, 0, null) { }

        // MÉTODOS DE NIVEL 

        public void SetLevel(int newLevel) => level = newLevel;

        private void ActualizarNivelYPropagar(Person persona, int nuevoNivel)
        {
            if (persona.level == nuevoNivel) return;

            persona.level = nuevoNivel;

            // Propagar a hijos
            foreach (var hijo in persona.children)
                ActualizarNivelYPropagar(hijo, nuevoNivel + 1);

            //  ACTUALIZAR también a la pareja para mantener consistencia
            if (persona.partner != null && persona.partner.level != nuevoNivel)
                persona.partner.level = nuevoNivel;
        }

        //  RELACIÓN: HIJOS 
        public void AddChild(Person child)
        {
            if (child == null || children.Contains(child)) return;

            children.Add(child);
            ActualizarNivelYPropagar(child, this.level + 1);

            // Agregar también a la pareja
            if (partner != null && !partner.children.Contains(child))
                partner.children.Add(child);
        }

        public void RemoveChild(Person child)
        {
            if (child != null) children.Remove(child);
        }

        public bool HasChildren()
        {
            return children.Any();
        }

        // RELACIÓN: PAREJA
        public bool CanAddPartner(Person newPartner)
        {
            return partner == null &&
                   newPartner != null &&
                   newPartner.partner == null &&
                   newPartner != this;
        }

        public void AddPartner(Person newPartner)
        {
            if (!CanAddPartner(newPartner)) return;

            partner = newPartner;
            newPartner.partner = this;

            // Asegurar que ambas tengan listas inicializadas
            if (newPartner.children == null)
                newPartner.children = new List<Person>();

            // Copiar hijos existentes de this a newPartner (sin duplicados)
            foreach (var hijo in this.children)
            {
                if (!newPartner.children.Contains(hijo))
                {
                    newPartner.children.Add(hijo);

                   //También agregar a newPartner como padre del hijo
                    if (hijo.CanAddParent())
                    {
                        hijo.AddParent(newPartner);
                    }
                }
            }

            // Copiar hijos de newPartner a this (sin duplicados)
            foreach (var hijo in newPartner.children)
            {
                if (!this.children.Contains(hijo))
                {
                    this.children.Add(hijo);

                    
                    if (hijo.CanAddParent())
                    {
                        hijo.AddParent(this);
                    }
                }
            }

            // Mantener niveles coherentes
            newPartner.level = this.level;
        }

        // RELACIÓN: PADRES

        public bool CanAddParent()
        {
            // PERMITIR agregar padres incluso si tiene pareja
            // Solo verificar que no tenga ya 2 padres
            return !(parents[0] != null && parents[1] != null);
        }

        public void AddParent(Person parent)
        {
            if (parent == null || !CanAddParent()) return;

            // Agregar padre
            if (parents[0] == null) parents[0] = parent;
            else if (parents[1] == null) parents[1] = parent;

            // Agregar hijo al padre y propagar niveles
            if (!parent.children.Contains(this))
            {
                parent.children.Add(this);
                parent.ActualizarNivelYPropagar(this, parent.level + 1);
            }

            // Agregar también al partner del padre
            if (parent.partner != null)
                VincularConPartnerDePadre(parent);

            // Si ya hay ambos padres, asegurarse que sean pareja
            if (parents[0] != null && parents[1] != null && parents[0].partner != parents[1])
                parents[0].AddPartner(parents[1]);
        }

        public bool HasParents()
        {
            return parents[0] != null  || parents[1] != null;
        }
        private void VincularConPartnerDePadre(Person parent)
        {
            var otro = parent.partner;

            if (!otro.children.Contains(this))
            {
                otro.children.Add(this);
                otro.ActualizarNivelYPropagar(this, otro.level + 1);
            }

            if (parents[0] != otro && parents[1] != otro)
            {
                if (parents[0] == null) parents[0] = otro;
                else if (parents[1] == null) parents[1] = otro;
            }
        }

        public void RemoveParent(Person parent)
        {
            if (parent == null) return;
            if (parents[0] == parent) parents[0] = null;
            else if (parents[1] == parent) parents[1] = null;
        }

        // VALIDACIONES 
        public bool FechasValidas()
        {
            if (birthdate > DateTime.Now) return false;
            if (deathDate.HasValue && deathDate.Value <= birthdate) return false;
            return true;
        }

        // GETTERS
        public Person[] Parents => parents;
        public List<Person> Children => children;
        public int GetLevel => level;
        public Person Partner => partner;
        public string GetName => name;
        public string GetId => id;

        public DateTime GetOldestParentBirthdate()
        {
            if (parents[0] != null && parents[1] == null) return parents[0].birthdate;
            if (parents[0] == null && parents[1] != null) return parents[1].birthdate;
            if (parents[0].birthdate < parents[1].birthdate) return parents[0].birthdate;
            return parents[1].birthdate;
        }

        public DateTime GetOldestChildBirthdate()
        {
            DateTime oldest = children[0].birthdate;
            foreach (var child in children)
            {
                if (child.birthdate < oldest)
                    oldest = child.birthdate;
            }
            return oldest;
        }
    }
}
