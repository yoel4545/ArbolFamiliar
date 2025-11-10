using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ArbolFamiliar;

namespace ArbolFamiliar //Se deberia agregar que verifica la edad al agregar un hijo o padre, que sea coherente. Faltan los metodos para cordenadas.
{
    internal class GrafoGenealogico
    {
        private Dictionary<Person, List<Person>> adyacencia;

        public GrafoGenealogico()
        {
            adyacencia = new Dictionary<Person, List<Person>>();
        }
        public void AddPerson(Person p) //Anadir persona al grafo
        {
            if (p == null)
                return;
            if (!adyacencia.ContainsKey(p))
                adyacencia[p] = new List<Person>();
        }

        public void AddChildren(Person father, Person child) //Agrega un hijo a un padre, crea las dos personas si no existen
        {
            if (!adyacencia.ContainsKey(father)) AddPerson(father);
            if (!adyacencia.ContainsKey(child)) AddPerson(child);
            father.AddChild(child);
            adyacencia[father].Add(child);
        }
        public void AddFather(Person child, Person father) //Agrega un padre a una persona ya existente
        {
            if (!adyacencia.ContainsKey(father)) AddPerson(father);
            if (!adyacencia.ContainsKey(child)) AddPerson(child);
            if (child == null || father == null)
                return;
            if (child.CanAddParent())
            {
                child.AddParent(father);
                father.AddChild(child);
                adyacencia[father].Add(child);
            }
        }

        public void AddPatner(Person existingPatner, Person newPatner)
        {
            existingPatner.AddPatner(newPatner);
            newPatner.AddPatner(existingPatner);
            newPatner.AddChildList(existingPatner);
        }

        public void DeletePerson(Person p) //Elimina una persona del grafo y conexiones con otras instancias de personas
        {
            if (p == null) return;
            if (adyacencia.ContainsKey(p))
            {
                // Eliminar nodo principal del grafo
                adyacencia.Remove(p);

                // Recorrer copia de las claves para evitar error
                var claves = new List<Person>(adyacencia.Keys);

                foreach (var key in claves)
                {
                    // Si p estaba como hijo de key
                    if (adyacencia[key].Contains(p))
                    {
                        adyacencia[key].Remove(p);
                        key.RemoveChild(p);
                    }

                    // Si p estaba como padre de key
                    if (key.Parents.Contains(p))
                    {
                        key.RemoveParent(p);
                    }
                }
            }
        }

        public List<Person> GetRoots() //Obtiene las personas que no son hijos de nadie, las personas raiz
        {
            var roots = new List<Person>(adyacencia.Keys);

            foreach (var children in adyacencia.Values) //Recorremos las listas de hijos de cada persona en el grefo
            {
                foreach (var child in children) //En la lista recorremos cada hijo
                {
                    roots.Remove(child);  //Removemos cada persona que sea hijo de alguien
                }
            }
            return roots;
        }

        public void CalculatePositions() //Hace un recorrido en anchura para asignar coordenadas a cada persona en el grafo
        {
            var roots = GetRoots();
            if (roots.Count == 0) return;

            Queue<(Person, int)> queue = new Queue<(Person, int)>(); //Cola en la que se agregan las personas que faltan por visitar, junto con su nivel en el arbol
            HashSet<Person> visited = new HashSet<Person>(); //Personas ya visitadas
            Dictionary<int, int> usedByLevel = new Dictionary<int, int>(); //Cuantas personas ya hay en cada nivel


            foreach (var root in roots) // Piner en cola todas las raíces en el nivel 0
            {
                queue.Enqueue((root, 0));
                visited.Add(root);
            }

            while (queue.Count > 0) //Recorrido de la cola
            {
                var (actual, level) = queue.Dequeue(); //Obtener persona actual y su nivel

                if (!usedByLevel.ContainsKey(level)) // Cuántas personas hay ya en este nivel
                    usedByLevel[level] = 0;

                // Calcular posición
                actual.x = usedByLevel[level] * 150; // separación horizontal
                actual.y = level * 120; // separación vertical
                usedByLevel[level]++;

                // Encolar hijos para el siguiente nivel
                if (adyacencia.ContainsKey(actual))
                {
                    foreach (var child in adyacencia[actual])
                    {
                        if (!visited.Contains(child))
                        {
                            queue.Enqueue((child, level + 1));
                            visited.Add(child);
                        }
                    }
                }
            }
        }

        public void Draw(Graphics g)
        {
            if (adyacencia.Count == 0) return;

            Font font = new Font("Arial", 9);
            Brush texto = Brushes.Black;
            Pen linea = Pens.Black;

            // 1️⃣ Dibujar conexiones (padres → hijos)
            foreach (var kvp in adyacencia)
            {
                var padre = kvp.Key;
                foreach (var hijo in kvp.Value)
                {
                    g.DrawLine(linea,
                        padre.x + 30, padre.y + 30,
                        hijo.x + 30, hijo.y + 30);
                }
            }

            // 2️⃣ Dibujar los nodos (personas)
            foreach (var kvp in adyacencia)
            {
                var p = kvp.Key;

                // Círculo para el nodo
                g.FillEllipse(Brushes.LightBlue, p.x, p.y, 60, 60);
                g.DrawEllipse(Pens.Black, p.x, p.y, 60, 60);

                // Nombre en el centro
                g.DrawString(p.GetName, font, texto, p.x + 5, p.y + 25);
            }
        }

    }
}
