using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArbolFamiliar;

namespace ArbolFamiliar //Se deberia agregar que verifica la edad al agregar un hijo o padre, que sea coherente. Faltan los metodos para cordenadas.
{
    internal class GrafoGenealogico
    {
        private Dictionary<Person, List<Person>> adyacencia;
        private int horizontalSpacing;
        private int verticalSpacing;
        private int radius; // Radio de los nodos circulares
        public GrafoGenealogico()
        {
            adyacencia = new Dictionary<Person, List<Person>>();
            horizontalSpacing = 75;
            verticalSpacing = 125;
            radius = 30;
        }
        public IEnumerable<Person> GetAllPersons()
        {
            return adyacencia.Keys;
        }

        public int Radius => radius;

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

            float hGap = horizontalSpacing;   // separación horizontal entre nodos
            float vGap = verticalSpacing;     // separación vertical entre niveles
            float coupleGap = Math.Max(hGap / 2f, 70f); // separación entre pareja

            // ---------------------------
            // Agrupar por nivel y obtener min/max nivel
            // ---------------------------
            var byLevel = adyacencia.Keys
                .GroupBy(p => p.GetLevel)
                .ToDictionary(g => g.Key, g => g.ToList());

            if (byLevel.Count == 0) return;

            int minLevel = byLevel.Keys.Min();
            int maxLevel = byLevel.Keys.Max();

            // Normalizar vertical: y = (level - minLevel) * vGap
            Func<int, int> CalcY = lvl => (int)Math.Round((lvl - minLevel) * vGap);

            // ---------------------------
            // 1) Colocar el nivel más bajo (maxLevel) de izquierda a derecha,
            //    agrupando hermanos por un padre encontrado (si existe)
            // ---------------------------
            float nextX = 0f;
            var placed = new HashSet<Person>();
            var bottom = byLevel.ContainsKey(maxLevel) ? byLevel[maxLevel] : new List<Person>();

            var handled = new HashSet<Person>();
            var groups = new List<List<Person>>();

            foreach (var child in bottom)
            {
                if (handled.Contains(child)) continue;

                // Buscar padres que tengan a 'child' en su Children (buscamos entre todas las personas del grafo)
                var parentsOfChild = adyacencia.Keys.Where(parent => parent.Children != null && parent.Children.Contains(child)).ToList();

                if (parentsOfChild.Count > 0)
                {
                    // Tomamos el primer padre encontrado para agrupar hermanos por ese padre
                    var parent = parentsOfChild[0];
                    var siblings = bottom.Where(c => parent.Children != null && parent.Children.Contains(c)).ToList();

                    foreach (var s in siblings) handled.Add(s);
                    groups.Add(siblings);
                }
                else
                {
                    // sin padre conocido -> grupo individual
                    groups.Add(new List<Person> { child });
                    handled.Add(child);
                }
            }

            // asignar posiciones X por grupo
            foreach (var grp in groups)
            {
                foreach (var ch in grp)
                {
                    ch.x = (int)Math.Round(nextX);
                    ch.y = CalcY(maxLevel);
                    placed.Add(ch);
                    nextX += hGap;
                }
                // espacio extra entre familias
                nextX += hGap * 0.5f;
            }

            // ---------------------------
            // 2) Subir niveles uno por uno (desde maxLevel-1 hasta minLevel)
            //    - Si un padre tiene hijos posicionados: centrar sobre ellos.
            //    - Si no tiene hijos posicionados: colocarlo a la derecha (nextX).
            //    - Si tiene pareja, colocar pareja centrada respecto al bloque de hijos (o al center).
            // ---------------------------
            for (int lvl = maxLevel - 1; lvl >= minLevel; lvl--)
            {
                if (!byLevel.ContainsKey(lvl)) continue;

                var persons = byLevel[lvl];

                foreach (var p in persons)
                {
                    var children = p.Children;

                    // sólo tomar hijos que ya fueron posicionados
                    var positionedChildren = (children ?? new List<Person>()).Where(c => placed.Contains(c)).ToList();

                    if (positionedChildren.Count > 0)
                    {
                        float minX = positionedChildren.Min(c => c.x);
                        float maxX = positionedChildren.Max(c => c.x);
                        float center = (minX + maxX) / 2f;

                        if (p.Patner != null)
                        {
                            // pareja centrada en center
                            p.x = (int)Math.Round(center - coupleGap / 2f);
                            p.y = CalcY(lvl);

                            p.Patner.x = (int)Math.Round(center + coupleGap / 2f);
                            p.Patner.y = CalcY(lvl);

                            placed.Add(p);
                            placed.Add(p.Patner);
                        }
                        else
                        {
                            // padre sin pareja, centrarlo
                            p.x = (int)Math.Round(center);
                            p.y = CalcY(lvl);
                            placed.Add(p);
                        }
                    }
                    else
                    {
                        // No tiene hijos posicionados (o no tiene hijos): ubicarlo en nextX
                        if (!placed.Contains(p))
                        {
                            p.x = (int)Math.Round(nextX);
                            p.y = CalcY(lvl);
                            placed.Add(p);
                            nextX += hGap;
                        }

                        // Si tiene pareja y no posicionada, colocarla al lado derecho
                        if (p.Patner != null && !placed.Contains(p.Patner))
                        {
                            p.Patner.x = (int)Math.Round(p.x + coupleGap);
                            p.Patner.y = CalcY(lvl);
                            placed.Add(p.Patner);
                            // avanzar nextX para reservar espacio
                            nextX = Math.Max(nextX, p.Patner.x + hGap);
                        }
                    }
                }
            }

            // Nota: No hacemos la parte 5 (no forzamos ubicar nodos restantes).
            // Asumimos que todos los nodos relevantes están en 'byLevel' y quedan posicionados.
        }

        public void DrawNodes(Graphics g)
        {
            if (adyacencia.Count == 0) return;

            Font font = new Font("Arial", 10);
            Brush texto = Brushes.Black;
            Pen linea = new Pen(Color.Black, 2);

            int diameter = radius * 2;

            // === 1️⃣ Dibujar líneas entre parejas ===
            foreach (var kvp in adyacencia)
            {
                var persona = kvp.Key;

                if (persona.Patner != null)
                {
                    // Línea entre los dos miembros de la pareja
                    float x1 = persona.x + radius;
                    float y1 = persona.y + radius;
                    float x2 = persona.Patner.x + radius;
                    float y2 = persona.Patner.y + radius;

                    g.DrawLine(linea, x1, y1, x2, y2);
                }
            }

            // === 2️⃣ Dibujar conexiones padres → hijos ===
            foreach (var kvp in adyacencia)
            {
                var padre = kvp.Key;
                var hijos = kvp.Value;

                if (hijos == null || hijos.Count == 0)
                    continue;

                // Punto de salida: si hay pareja, desde el centro entre ambos
                float xOrigen, yOrigen;

                if (padre.Patner != null)
                {
                    xOrigen = (padre.x + padre.Patner.x) / 2f + radius;
                    yOrigen = padre.y + radius;
                }
                else
                {
                    xOrigen = padre.x + radius;
                    yOrigen = padre.y + radius;
                }

                // Dibujar línea vertical desde el padre (o pareja) hacia los hijos
                float yConexion = hijos[0].y - radius - 20; // un poco arriba del nivel hijo
                g.DrawLine(linea, xOrigen, yOrigen, xOrigen, yConexion);

                // Línea horizontal que conecta a los hijos
                float minX = hijos.Min(h => h.x + radius);
                float maxX = hijos.Max(h => h.x + radius);
                g.DrawLine(linea, minX, yConexion, maxX, yConexion);

                // Dibujar líneas verticales hacia cada hijo
                foreach (var h in hijos)
                {
                    float xHijo = h.x + radius;
                    float yHijo = h.y;
                    g.DrawLine(linea, xHijo, yConexion, xHijo, yHijo);
                }
            }

            // === 3️⃣ Dibujar los nodos (personas) ===
            foreach (var kvp in adyacencia)
            {
                var p = kvp.Key;

                // Círculo
                g.FillEllipse(Brushes.LightBlue, p.x, p.y, diameter, diameter);
                g.DrawEllipse(Pens.Black, p.x, p.y, diameter, diameter);

                // Nombre centrado
                var textSize = g.MeasureString(p.GetName, font);
                float textX = p.x + radius - textSize.Width / 2;
                float textY = p.y + radius - textSize.Height / 2;
                g.DrawString(p.GetName, font, texto, textX, textY);
            }
        }
    }
}
