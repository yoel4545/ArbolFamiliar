using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using ArbolFamiliar;

namespace ArbolFamiliar //Se deberia agregar que verifica la edad al agregar un hijo o padre, que sea coherente. Faltan los metodos para cordenadas.
{
    internal class GrafoGenealogico
    {
        private Dictionary<Person, List<Person>> adyacencia;
        private int horizontalSpacing;
        private int verticalSpacing;
        public GrafoGenealogico()
        {
            adyacencia = new Dictionary<Person, List<Person>>();
            horizontalSpacing = 150;
            verticalSpacing = 200;
        }
        public void AddPerson(Person p) //Anadir persona al grafo
        {
            if (p == null)
                return;
            if (!adyacencia.ContainsKey(p))
                adyacencia[p] = new List<Person>();
            CalculatePositions();
        }

        public void AddChildren(Person father, Person child) //Agrega un hijo a un padre, crea las dos personas si no existen
        {
            if (!adyacencia.ContainsKey(father)) AddPerson(father);
            if (!adyacencia.ContainsKey(child)) AddPerson(child);
            father.AddChild(child);
            adyacencia[father].Add(child);
            CalculatePositions();
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
            CalculatePositions();
        }

        public void AddPatner(Person existingPatner, Person newPatner)
        {
            if (!adyacencia.ContainsKey(existingPatner)) AddPerson(existingPatner);
            if (!adyacencia.ContainsKey(newPatner)) AddPerson(newPatner);
            existingPatner.AddPatner(newPatner);
            newPatner.AddPatner(existingPatner);
            newPatner.AddChildList(existingPatner);
            CalculatePositions();
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

        public void CalculatePositions()
        {
            if (adyacencia == null || adyacencia.Count == 0) return;

            // Parámetros de layout (ajusta si quieres más o menos separación)
            float baseWidth = horizontalSpacing; // unidad mínima de ancho para un nodo
            float hGap = horizontalSpacing;
            float vGap = verticalSpacing;
            float coupleGap = Math.Min(80, hGap / 2f);

            // --------------------------
            // 1) Asegurar niveles (BFS desde raíces)
            // --------------------------
            var roots = GetRoots();
            var queue = new Queue<Person>();
            var seen = new HashSet<Person>();

            foreach (var r in roots)
            {
                r.SetLevel(0);
                queue.Enqueue(r);
                seen.Add(r);
            }

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                int nivel = cur.GetLevel;

                // hijos directos (si existen)
                if (adyacencia.ContainsKey(cur))
                {
                    foreach (var ch in adyacencia[cur])
                    {
                        // si el hijo no tiene nivel definido o es más grande que el correcto, asignar
                        ch.SetLevel(nivel + 1);
                        if (!seen.Contains(ch))
                        {
                            seen.Add(ch);
                            queue.Enqueue(ch);
                        }
                    }
                }

                // si tiene pareja que no tiene nivel, mantener la pareja en el mismo nivel
                if (cur.Patner != null && !seen.Contains(cur.Patner))
                {
                    cur.Patner.SetLevel(nivel);
                    seen.Add(cur.Patner);
                    queue.Enqueue(cur.Patner);
                }
            }

            // --------------------------
            // 2) Calcular ancho (width) de cada subárbol (basado en hijos)
            // --------------------------
            var widths = new Dictionary<Person, float>();
            var visitedWidth = new HashSet<Person>();

            foreach (var r in roots)
                ComputeSubtreeWidth(r, widths, baseWidth, hGap, new HashSet<Person>());

            // Puede haber nodos no alcanzables desde roots (aislados) — asegurarles width
            foreach (var p in adyacencia.Keys)
            {
                if (!widths.ContainsKey(p))
                    ComputeSubtreeWidth(p, widths, baseWidth, hGap, new HashSet<Person>());
            }

            // --------------------------
            // 3) Colocar subárboles top-down (cada raíz en orden, desde cursorX hacia la derecha)
            // --------------------------
            var positioned = new HashSet<Person>();
            float cursorX = 0f;

            foreach (var r in roots)
            {
                float w = widths.ContainsKey(r) ? widths[r] : baseWidth;
                PlaceSubtree(r, cursorX, widths, hGap, vGap, coupleGap, positioned);
                cursorX += w + hGap; // espacio entre familias
            }

            // 4) Si quedan nodos no posicionados (por seguridad), colocarlos a la derecha
            var unpositioned = adyacencia.Keys.Where(p => !positioned.Contains(p)).ToList();
            if (unpositioned.Count > 0)
            {
                foreach (var p in unpositioned)
                {
                    p.x = (int)Math.Round(cursorX);
                    p.y = p.GetLevel * (int)vGap;
                    cursorX += hGap;
                    positioned.Add(p);
                }
            }
        }

        // --------------------------
        // Helper: calcula el ancho del subárbol de 'node' (considera pareja y unión de hijos)
        // --------------------------
        private float ComputeSubtreeWidth(Person node, Dictionary<Person, float> widths, float baseWidth, float hGap, HashSet<Person> seen)
        {
            if (widths.ContainsKey(node))
                return widths[node];

            if (seen.Contains(node))
            {
                // ciclo improbable: devolver baseWidth para evitar bucles infinitos
                widths[node] = baseWidth;
                return baseWidth;
            }
            seen.Add(node);

            // hijos combinados (considerando pareja)
            var children = GetCombinedChildren(node, node.Patner);

            if (children == null || children.Count == 0)
            {
                widths[node] = baseWidth;
                // si tiene pareja, pareja comparte el mismo ancho
                if (node.Patner != null && !widths.ContainsKey(node.Patner))
                    widths[node.Patner] = baseWidth;
                return baseWidth;
            }

            // sumar anchos recursivamente
            float totalChildrenWidth = 0f;
            for (int i = 0; i < children.Count; i++)
            {
                var ch = children[i];
                float wch = ComputeSubtreeWidth(ch, widths, baseWidth, hGap, seen);
                totalChildrenWidth += wch;
            }

            // añadir gaps entre hijos
            totalChildrenWidth += hGap * (children.Count - 1);

            float nodeWidth = Math.Max(baseWidth, totalChildrenWidth);

            widths[node] = nodeWidth;
            if (node.Patner != null && !widths.ContainsKey(node.Patner))
                widths[node.Patner] = nodeWidth;

            return nodeWidth;
        }

        // --------------------------
        // Helper: coloca el subárbol de 'node' dentro del intervalo [left, left + widths[node])
        // --------------------------
        private void PlaceSubtree(Person node, float left, Dictionary<Person, float> widths, float hGap, float vGap, float coupleGap, HashSet<Person> positioned)
        {
            if (positioned.Contains(node)) return;

            float nodeWidth = widths.ContainsKey(node) ? widths[node] : hGap;
            float center = left + nodeWidth / 2f;

            int lvl = node.GetLevel;
            float y = lvl * vGap;

            // si tiene pareja y la pareja no está posicionada, colocarlos alrededor del centro
            if (node.Patner != null && !positioned.Contains(node.Patner))
            {
                var partner = node.Patner;

                node.x = (int)Math.Round(center - coupleGap / 2f);
                node.y = (int)Math.Round(y);

                partner.x = (int)Math.Round(center + coupleGap / 2f);
                partner.y = (int)Math.Round(y);

                positioned.Add(node);
                positioned.Add(partner);

                // hijos combinados
                var children = GetCombinedChildren(node, partner);
                if (children == null || children.Count == 0) return;

                float childLeft = left;
                for (int i = 0; i < children.Count; i++)
                {
                    var ch = children[i];
                    float wch = widths.ContainsKey(ch) ? widths[ch] : hGap;
                    PlaceSubtree(ch, childLeft, widths, hGap, vGap, coupleGap, positioned);
                    childLeft += wch + hGap;
                }
            }
            else
            {
                // nodo sin pareja (o su pareja ya posicionada)
                node.x = (int)Math.Round(center);
                node.y = (int)Math.Round(y);
                positioned.Add(node);

                var children = adyacencia.ContainsKey(node) ? adyacencia[node] : new List<Person>();
                if (children == null || children.Count == 0) return;

                float childLeft = left;
                for (int i = 0; i < children.Count; i++)
                {
                    var ch = children[i];
                    float wch = widths.ContainsKey(ch) ? widths[ch] : hGap;
                    PlaceSubtree(ch, childLeft, widths, hGap, vGap, coupleGap, positioned);
                    childLeft += wch + hGap;
                }
            }
        }

        // --------------------------
        // Helper: devuelve la unión de hijos de a y b (si existen), sin duplicados
        // --------------------------
        private List<Person> GetCombinedChildren(Person a, Person b)
        {
            var list = new List<Person>();
            if (a != null && adyacencia.ContainsKey(a)) list.AddRange(adyacencia[a]);
            if (b != null && adyacencia.ContainsKey(b)) list.AddRange(adyacencia[b]);
            return list.Distinct().ToList();
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
                Debug.WriteLine("Dibujando" + p.name + "en" + p.x + p.y);

                // Nombre en el centro
                g.DrawString(p.GetName, font, texto, p.x + 5, p.y + 25);
            }
        }

    }
}
