using System;
using System.Collections.Generic;

namespace ArbolFamiliar
{
    public class Persona
    {
        public string name { get; set; }
        public string id { get; set; }
        private DateTime birthdate { get; set; }
        private DateTime? deathDate { get; set; }
        public string fotoPath { get; set; }
        private List<Persona> children { get; set; }
        private Persona[] parents { get; set; }
        public int x { get; set; } //Coordenada x para graficar
        public int y { get; set; } //Coordenada y para graficar
        public double Latitud { get; set; }
        public double Longitud { get; set; }

        public Persona(string name, string id, DateTime birthdate, string photoPath, double lat, double lng) //Metodo constructor con informacion basica, para un familiar vivo
        {
            this.name = name;
            this.id = id;
            this.birthdate = birthdate;
            deathDate = null;
            this.fotoPath = photoPath;
            children = new List<Persona>();
            parents = new Persona[2]; //Maximo dos padres
            this.Latitud = lat;
            this.Longitud = lng;
        }

        public Persona(string name, string id, DateTime birthdate, string photoPath, DateTime deathDate) //Metodo constructor con informacion basica, para un familiar fallecido
        {
            this.name = name;
            this.id = id;
            this.birthdate = birthdate;
            this.deathDate = deathDate;
            this.fotoPath = photoPath;
            children = new List<Persona>();
            parents = new Persona[2]; //Maximo dos padres
        }

        public void AddChild(Persona child) //Anade un hijo a la lista de hijos propia
        {
            if (children == null)
            {
                return;
            }
            if (child != null && !children.Contains(child))
            {
                children.Add(child);
            }
        }

        public bool CanAddParent() //Revisa si se puede anadir un padre, dos disponibles los cuales son nulos si hay espacio
        {
            if (parents[0] == null || parents[1] == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddParent(Persona parent) //Anade un padre a la lista de padres propia
        {
            if (parent != null)
            {
                if (parents[0] == null)
                {
                    parents[0] = parent;
                }
                else if (parents[1] == null)
                {
                    parents[1] = parent;
                }
            }
        }

        public void RemoveChild(Persona child) //Elimina un hijo de la lista de hijos propia
        {
            if (child != null) return;
            if (children.Contains(child))
            {
                children.Remove(child);
            }
        }

        public void RemoveParent(Persona parent) //Elimina un padre de la lista de padres propia
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

        public Persona[] Parents => parents;

        public string GetName
        {
            get => name;
        }
    }
}