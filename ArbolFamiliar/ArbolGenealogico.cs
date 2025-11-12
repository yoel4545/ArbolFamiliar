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
            if (father.partner != null)
            {
                father.partner.AddChild(child);
                adyacencia[father.partner].Add(child);
            }
            CalculatePositions();
        }
        public void AddParent(Person child, Person father) //Agrega un padre a una persona ya existente
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

            // Verificación de disponibilidad antes de enlazar
            if (!existingPatner.CanAddPartner(newPatner))
            {
                return;
            }

            existingPatner.AddPartner(newPatner);
            CalculatePositions();
        }


        public void DeletePerson(Person p)
        {
            if (p == null) return;
            if (!adyacencia.ContainsKey(p)) return;
            if (adyacencia.Count() <= 1) return;

            // --- 1. Si tiene pareja, eliminar también a la pareja ---
            if (p.partner != null)
            {
                var pareja = p.partner;
                p.partner = null;
                pareja.partner = null;

                // Llamada recursiva para eliminar la pareja
                DeletePerson(pareja);
            }

            // --- 2. Eliminar recursivamente todos los hijos ---
            if (p.Children != null)
            {
                // Hacemos una copia para evitar modificación durante la iteración
                var hijos = new List<Person>(p.Children);
                foreach (var hijo in hijos)
                {
                    DeletePerson(hijo);
                }
            }

            // --- 3. Limpiar referencias de los padres ---
            if (p.Parents != null)
            {
                foreach (var padre in p.Parents)
                {
                    if (padre != null)
                    {
                        padre.RemoveChild(p);
                    }
                }
            }

            // --- 4. Eliminar nodo del grafo ---
            adyacencia.Remove(p);

            // --- 5. Limpiar referencias desde otros nodos ---
            var claves = new List<Person>(adyacencia.Keys);
            foreach (var key in claves)
            {
                if (adyacencia[key].Contains(p))
                {
                    adyacencia[key].Remove(p);
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
            if (adyacencia == null || adyacencia.Count == 0)
            {
                Person p = new Person("N/A", "0000", DateTime.Now, "");
                AddPerson(p);
            }

            int ConsistentRound(float value)
            {
                return (int)Math.Floor(value + 0.5f);
            }

            float hGap = horizontalSpacing;   // separación horizontal básica (unidad)
            float vGap = verticalSpacing;     // separación vertical
            float coupleGap = Math.Max(hGap / 2f, 70f); // separación pareja dentro del bloque

            // 1) agrupar por nivel y calcular min/max
            var byLevel = adyacencia.Keys.GroupBy(p => p.GetLevel)
                             .ToDictionary(g => g.Key, g => g.ToList());

            if (byLevel.Count == 0) return;

            int minLevel = byLevel.Keys.Min();
            int maxLevel = byLevel.Keys.Max();

            Func<int, int> CalcY = lvl => ConsistentRound((lvl - minLevel) * vGap);

            // 2) calcular ancho por subárbol (recursivo) - ahora usando "slot" por hijo
            var widths = new Dictionary<Person, float>();
            var visiting = new HashSet<Person>();

            float ComputeWidth(Person node)
            {
                if (widths.ContainsKey(node)) return widths[node];
                if (visiting.Contains(node))
                {
                    widths[node] = hGap;
                    return widths[node];
                }
                visiting.Add(node);

                var children = node.Children ?? new List<Person>();

                if (children.Count == 0)
                {
                    // hoja: reservar 2 ranuras si tiene pareja, si no 1 ranura
                    float leafWidth = (node.Partner != null) ? 2f * hGap : 1f * hGap;
                    widths[node] = leafWidth;
                    if (node.Partner != null && !widths.ContainsKey(node.Partner))
                        widths[node.Partner] = leafWidth;
                    visiting.Remove(node);
                    return leafWidth;
                }

                // Sumar "slot" de cada hijo (slot = max(widthHijo, (pareja?2:1)*hGap))
                float total = 0f;
                for (int i = 0; i < children.Count; i++)
                {
                    var ch = children[i];
                    float wch = ComputeWidth(ch);
                    float slot = Math.Max(wch, (ch.Partner != null) ? 2f * hGap : 1f * hGap);
                    total += slot;
                }
                // gaps entre ranuras
                total += hGap * (children.Count - 1);

                float nodeWidth = Math.Max(1f * hGap, total);
                widths[node] = nodeWidth;
                if (node.Partner != null && !widths.ContainsKey(node.Partner))
                    widths[node.Partner] = nodeWidth;

                visiting.Remove(node);
                return nodeWidth;
            }

            // calcular para todas las raíces primero (por si hay varias)
            var roots = GetRoots();
            foreach (var r in roots) ComputeWidth(r);
            // asegurar todos los nodos tengan width
            foreach (var p in adyacencia.Keys) if (!widths.ContainsKey(p)) ComputeWidth(p);

            // 3) colocar subárboles top-down usando los anchos calculados
            var placed = new HashSet<Person>();
            float cursorX = 0f;

            void PlaceSubtree(Person node, float left)
            {
                if (placed.Contains(node)) return;
                float nodeWidth = widths.ContainsKey(node) ? widths[node] : hGap;
                int lvl = node.GetLevel;
                float y = CalcY(lvl);

                // obtener hijos y sus "slots"
                var children = node.Children ?? new List<Person>();
                var childSlots = new List<float>();
                for (int i = 0; i < children.Count; i++)
                {
                    var ch = children[i];
                    float wch = widths.ContainsKey(ch) ? widths[ch] : hGap;
                    float slot = Math.Max(wch, (ch.Partner != null) ? 2f * hGap : 1f * hGap);
                    childSlots.Add(slot);
                }

                // calcular centros "efectivos" de cada hijo dentro del intervalo [left, ...]
                var childCenters = new List<float>();
                float childLeftCursor = left;
                for (int i = 0; i < children.Count; i++)
                {
                    var ch = children[i];
                    float slot = childSlots[i];

                    // si el hijo tiene pareja y slot==2*hGap, su "centro efectivo" para centrar al padre
                    // es la primera ranura (childLeft + 0.5*hGap); si no, el centro es slot/2.
                    if (ch.Partner != null && Math.Abs(slot - 2f * hGap) < 1e-6)
                    {
                        childCenters.Add(childLeftCursor + 0.5f * hGap);
                    }
                    else
                    {
                        // centro del sub-slot
                        childCenters.Add(childLeftCursor + slot / 2f);
                    }

                    childLeftCursor += slot + hGap;
                }

                // Decide centro para el nodo según hijos existentes
                float centerForNode;
                if (childCenters.Count == 0)
                {
                    centerForNode = left + nodeWidth / 2f;
                }
                else if (childCenters.Count == 1)
                {
                    // caso crítico: hijo único
                    centerForNode = childCenters[0]; // si hijo tiene pareja, childCenters[0] ya apunta a la ranura correcta
                }
                else
                {
                    float minC = childCenters.Min();
                    float maxC = childCenters.Max();
                    centerForNode = (minC + maxC) / 2f;
                }

                // Colocar el nodo y su pareja (si la tiene)
                if (node.Partner != null && !placed.Contains(node.Partner))
                {
                    // si es hoja (no hijos), colocamos hijo y pareja en las dos ranuras dentro del left asignado
                    if (children.Count == 0)
                    {
                        node.x = ConsistentRound(left + 0.5f * hGap);
                        node.y = ConsistentRound(y);
                        node.Partner.x = ConsistentRound(left + 1.5f * hGap);
                        node.Partner.y = ConsistentRound(y);

                        placed.Add(node);
                        placed.Add(node.Partner);
                        return;
                    }
                    else
                    {
                        // pareja con hijos: repartir alrededor de centerForNode
                        node.x = ConsistentRound(centerForNode - coupleGap / 2f);
                        node.y = ConsistentRound(y);

                        node.Partner.x = ConsistentRound(centerForNode + coupleGap / 2f);
                        node.Partner.y = ConsistentRound(y);

                        placed.Add(node);
                        placed.Add(node.Partner);
                    }
                }
                else
                {
                    node.x = ConsistentRound(centerForNode);
                    node.y = ConsistentRound(y);
                    placed.Add(node);
                }

                // finalmente colocamos recursivamente a los hijos dentro de sus ranuras
                float cLeft = left;
                for (int i = 0; i < children.Count; i++)
                {
                    var ch = children[i];
                    float slot = childSlots[i];
                    float wch = widths.ContainsKey(ch) ? widths[ch] : hGap;

                    // centramos el subárbol del hijo dentro de su ranura (si su ancho real < slot)
                    float childInnerLeft = cLeft + (slot - wch) / 2f;
                    PlaceSubtree(ch, childInnerLeft);

                    cLeft += slot + hGap;
                }
            }

            // colocar cada raíz en orden
            foreach (var r in roots)
            {
                float w = widths.ContainsKey(r) ? widths[r] : hGap;
                PlaceSubtree(r, cursorX);
                cursorX += w + hGap; // espacio entre familias
            }

            // por seguridad: si queda algún nodo sin colocar, ponerlos a la derecha
            foreach (var p in adyacencia.Keys)
            {
                if (!placed.Contains(p))
                {
                    p.x = ConsistentRound(cursorX);
                    p.y = CalcY(p.GetLevel);
                    cursorX += hGap;
                }
            }
        }



        public void DrawNodes(Graphics g)
        {
            if (adyacencia.Count == 0) return;

            Font font = new Font("Arial", 10);
            Brush texto = Brushes.Black;
            Pen linea = new Pen(Color.Black, 4);

            int diameter = radius * 2;

            // === 1️⃣ Dibujar líneas entre parejas ===
            foreach (var kvp in adyacencia)
            {
                var persona = kvp.Key;

                if (persona.Partner != null)
                {
                    // Línea entre los dos miembros de la pareja
                    float x1 = persona.x + radius;
                    float y1 = persona.y + radius;
                    float x2 = persona.Partner.x + radius;
                    float y2 = persona.Partner.y + radius;

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

                if (padre.Partner != null)
                {
                    xOrigen = (padre.x + padre.Partner.x) / 2f + radius;
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
