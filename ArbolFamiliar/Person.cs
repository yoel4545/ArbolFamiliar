using System;
using System.Collections.Generic;

namespace Arbol_1
{
    internal class Person
    {
        private string name { get; set; }
        private string id { get; set; }
        private DateTime birthdate { get; set; }
        private DateTime? deathDate { get; set; }
        public string fotoPath { get; set; }
        private List<Person> children { get; set; }
        private Person[] parents { get; set; }
        public int x { get; set; } //Coordenada x para graficar
        public int y { get; set; } //Coordenada y para graficar
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
            this.Latitud = lat;
            this.Longitud = lng;
        }

        public Person(string name, string id, DateTime birthdate, string photoPath, DateTime deathDate) //Metodo constructor con informacion basica, para un familiar fallecido
        {
            this.name = name;
            this.id = id;
            this.birthdate = birthdate;
            this.deathDate = deathDate;
            this.fotoPath = photoPath;
            children = new List<Person>();
            parents = new Person[2]; //Maximo dos padres
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

        public void AddParent(Person parent) //Anade un padre a la lista de padres propia
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

        public string GetName
        {
            get => name;
        }
    }
}