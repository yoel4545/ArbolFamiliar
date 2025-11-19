using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArbolFamiliar;

namespace ArbolFamiliar //Se deberia agregar que verifica la edad al agregar un hijo o padre, que sea coherente. Faltan los metodos para cordenadas.
{
    public class GrafoGenealogico
    {
        private Dictionary<Person, List<Person>> adyacencia;
        private int horizontalSpacing;
        private int verticalSpacing;
        private int radius; // Radio de los nodos circulares
        public GrafoGenealogico()
        {
            adyacencia = new Dictionary<Person, List<Person>>();
            horizontalSpacing = 100;
            verticalSpacing = 175;
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

        public void AddChildren(Person father, Person child) 
        {
            if (!adyacencia.ContainsKey(father)) AddPerson(father);
            if (!adyacencia.ContainsKey(child)) AddPerson(child);

         
            father.AddChild(child);

          
            if (child.CanAddParent())
            {
                child.AddParent(father);
            }

            
            adyacencia[father].Add(child);

         
            if (father.partner != null)
            {
                father.partner.AddChild(child);

               
                if (child.CanAddParent())
                {
                    child.AddParent(father.partner);
                }

                adyacencia[father.partner].Add(child);
            }

            CalculatePositions();
        }
        public void AddParent(Person child, Person father) //Agrega un padre a una persona ya existente
        {
            if (child == null || father == null)
                return;
            if (!adyacencia.ContainsKey(father)) AddPerson(father);
            if (!adyacencia.ContainsKey(child)) AddPerson(child);
            if (child.CanAddParent())
            {
                child.AddParent(father);
                father.AddChild(child);
                adyacencia[father].Add(child);
            }
            else
            {
                adyacencia.Remove(father);
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
                adyacencia.Remove(newPatner);
                return;
            }

            existingPatner.AddPartner(newPatner);

            foreach (var hijo in existingPatner.Children)
            {
                // Asegurar que el hijo esté en adyacencia
                if (!adyacencia.ContainsKey(hijo))
                    adyacencia[hijo] = new List<Person>();

                // Enlazar al nuevo padre/madre
                if (!adyacencia[newPatner].Contains(hijo))
                    adyacencia[newPatner].Add(hijo);
            }

            CalculatePositions();
        }
        private static bool HasParents(Person x) =>
    x?.Parents != null && x.Parents.Any(p => p != null);

        // Recolecta componente: nodo, sus hijos recursivos y parejas (y sus descendientes)
        private HashSet<Person> CollectComponent(Person root)
        {
            var toDelete = new HashSet<Person>();
            if (root == null) return toDelete;

            var stack = new Stack<Person>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var n = stack.Pop();
                if (n == null || !toDelete.Add(n)) continue;

                if (n.Children != null)
                    foreach (var ch in n.Children) if (ch != null) stack.Push(ch);

                if (n.Partner != null) stack.Push(n.Partner);
            }
            return toDelete;
        }

        // Borra completamente todos los nodos del componente (limpia parents/children/partner/adyacencia)
        private void DeleteComponent(Person root)
        {
            var toDelete = CollectComponent(root);
            if (toDelete.Count == 0) return;

            // 1) Quitar referencias desde padres externos y limpiar Parents
            foreach (var node in toDelete)
            {
                if (node.Parents == null) continue;
                for (int i = 0; i < node.Parents.Length; i++)
                {
                    var padre = node.Parents[i];
                    if (padre != null && !toDelete.Contains(padre))
                    {
                        padre.Children.Remove(node);
                        if (adyacencia.ContainsKey(padre)) adyacencia[padre].Remove(node);
                    }
                    node.Parents[i] = null;
                }
            }

            // 2) Romper parejas recíprocas
            foreach (var node in toDelete)
            {
                if (node.partner != null)
                {
                    if (node.partner.partner == node) node.partner.partner = null;
                    node.partner = null;
                }
            }

            // 3) Eliminar entradas del diccionario y limpiar referencias desde nodos que quedan
            foreach (var node in toDelete)
                if (adyacencia.ContainsKey(node)) adyacencia.Remove(node);

            var keys = new List<Person>(adyacencia.Keys);
            foreach (var k in keys)
            {
                // eliminar referencias colgantes en adyacencia y children
                foreach (var dead in toDelete)
                {
                    adyacencia[k].Remove(dead);
                    k.Children.Remove(dead);
                }
            }
        }

        // Elimina solo el nodo p (sin tocar sus descendientes)
        private void RemoveSingleNode(Person p)
        {
            if (p == null || !adyacencia.ContainsKey(p)) return;

            // quitar de padres
            if (p.Parents != null)
            {
                foreach (var padre in p.Parents)
                {
                    if (padre == null) continue;
                    padre.Children.Remove(p);
                    if (adyacencia.ContainsKey(padre)) adyacencia[padre].Remove(p);
                }
                for (int i = 0; i < p.Parents.Length; i++) p.Parents[i] = null;
            }

            // romper pareja recíproca
            if (p.partner != null)
            {
                if (p.partner.partner == p) p.partner.partner = null;
                p.partner = null;
            }

            // limpiar referencias en hijos (sin borrar hijos)
            if (p.Children != null)
            {
                foreach (var ch in p.Children)
                    if (ch?.Parents != null)
                        for (int i = 0; i < ch.Parents.Length; i++)
                            if (ch.Parents[i] == p) ch.Parents[i] = null;
            }

            // eliminar del diccionario y limpiar referencias externas
            adyacencia.Remove(p);
            var keys = new List<Person>(adyacencia.Keys);
            foreach (var k in keys)
            {
                adyacencia[k].Remove(p);
                k.Children.Remove(p);
            }
        }

        // DeletePerson: lógica compacta para tus 4 casos
        public void DeletePerson(Person p)
        {
            if (p == null || !adyacencia.ContainsKey(p)) return;

            bool hasParents = HasParents(p);
            bool hasPartner = p.Partner != null;

            if (!hasParents && !hasPartner)          // Caso 1
                DeleteComponent(p);
            else if (!hasParents && hasPartner)      // Caso 2
            {
                var pareja = p.Partner;
                if (!adyacencia.ContainsKey(pareja)) AddPerson(pareja);

                // reasignar hijos a la pareja
                foreach (var h in new List<Person>(p.Children))
                {
                    if (h.Parents != null)
                        for (int i = 0; i < h.Parents.Length; i++)
                            if (h.Parents[i] == p) h.Parents[i] = pareja;

                    if (!pareja.Children.Contains(h)) pareja.AddChild(h);
                    if (!adyacencia.ContainsKey(pareja)) adyacencia[pareja] = new List<Person>();
                    if (!adyacencia[pareja].Contains(h)) adyacencia[pareja].Add(h);
                }

                // romper pareja y quitar p
                if (pareja.partner == p) pareja.partner = null;
                p.partner = null;
                RemoveSingleNode(p);
            }
            else if (hasParents && hasPartner)       // Caso 3
            {
                var pareja = p.Partner;
                if (p.partner == pareja) p.partner = null;
                if (pareja.partner == p) pareja.partner = null;
                DeleteComponent(pareja);
            }
            else /* hasParents && !hasPartner */    // Caso 4
                DeleteComponent(p);

            RebuildAdjacency();
            CalculatePositions();
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

            int ConsistentRound(float value) => (int)Math.Floor(value + 0.5f);

            float hGap = horizontalSpacing;
            float vGap = verticalSpacing;
            float coupleGap = Math.Max(hGap * 1.5f, 120f);

            // =====================================================================
            // (0) Recalcular niveles globales: tratar parejas como una sola componente
            // =====================================================================

            var allNodes = new HashSet<Person>(adyacencia.Keys);
            foreach (var kv in adyacencia)
            {
                foreach (var ch in kv.Value)
                    if (ch != null) allNodes.Add(ch);
                if (kv.Key.Partner != null) allNodes.Add(kv.Key.Partner);
            }

            // --- Union-Find para agrupar parejas ---
            var parentMap = new Dictionary<Person, Person>();
            Person Find(Person a)
            {
                if (!parentMap.ContainsKey(a)) parentMap[a] = a;
                if (parentMap[a] == a) return a;
                parentMap[a] = Find(parentMap[a]);
                return parentMap[a];
            }
            void Union(Person a, Person b)
            {
                if (a == null || b == null) return;
                var ra = Find(a);
                var rb = Find(b);
                if (ra != rb) parentMap[rb] = ra;
            }

            foreach (var n in allNodes) if (!parentMap.ContainsKey(n)) parentMap[n] = n;
            foreach (var n in allNodes)
                if (n?.Partner != null) Union(n, n.Partner);

            // --- Construir componentes (representante -> miembros) ---
            var compMembers = new Dictionary<Person, List<Person>>();
            foreach (var n in allNodes)
            {
                var r = Find(n);
                if (!compMembers.ContainsKey(r)) compMembers[r] = new List<Person>();
                compMembers[r].Add(n);
            }

            // --- Crear grafo de componentes ---
            var compIndex = new Dictionary<Person, int>();
            int cidx = 0;
            foreach (var rep in compMembers.Keys) compIndex[rep] = cidx++;

            var compIndegree = new int[compMembers.Count];
            var compAdj = new List<HashSet<int>>();
            for (int i = 0; i < compMembers.Count; i++) compAdj.Add(new HashSet<int>());

            var personToRep = new Dictionary<Person, Person>();
            foreach (var kv in compMembers)
                foreach (var mem in kv.Value)
                    personToRep[mem] = kv.Key;

            foreach (var kv in adyacencia)
            {
                var parent = kv.Key;
                if (!personToRep.ContainsKey(parent)) continue;
                var children = kv.Value ?? new List<Person>();
                int pr = compIndex[personToRep[parent]];

                foreach (var ch in children)
                {
                    if (ch == null || !personToRep.ContainsKey(ch)) continue;
                    int cr = compIndex[personToRep[ch]];
                    if (pr == cr) continue;
                    if (compAdj[pr].Add(cr))
                        compIndegree[cr]++;
                }
            }

            // --- Kahn: topological sort sobre componentes ---
            var compLevel = new int[compMembers.Count];
            for (int i = 0; i < compLevel.Length; i++) compLevel[i] = int.MinValue;

            var q = new Queue<int>();
            for (int i = 0; i < compIndegree.Length; i++)
                if (compIndegree[i] == 0) { compLevel[i] = 0; q.Enqueue(i); }

            while (q.Count > 0)
            {
                int cur = q.Dequeue();
                foreach (var nxt in compAdj[cur])
                {
                    int candidate = compLevel[cur] + 1;
                    if (candidate > compLevel[nxt]) compLevel[nxt] = candidate;
                    compIndegree[nxt]--;
                    if (compIndegree[nxt] == 0) q.Enqueue(nxt);
                }
            }

            for (int i = 0; i < compLevel.Length; i++)
            {
                if (compLevel[i] != int.MinValue) continue;
                int assigned = int.MaxValue;
                for (int j = 0; j < compAdj.Count; j++)
                {
                    if (compAdj[j].Contains(i) && compLevel[j] != int.MinValue)
                        assigned = Math.Min(assigned, compLevel[j] + 1);
                }
                if (assigned == int.MaxValue) assigned = 0;
                compLevel[i] = assigned;
            }

            // --- Asignar niveles a cada persona ---
            foreach (var kv in compMembers)
            {
                var rep = kv.Key;
                int lvl = compLevel[compIndex[rep]];
                foreach (var mem in kv.Value)
                {
                    if (mem != null) mem.SetLevel(lvl);
                }
            }

            DebugPrintComponents(compMembers, compIndex, compLevel);

            // =====================================================================
            // (1) Agrupar por nivel
            // =====================================================================
            var byLevel = adyacencia.Keys.GroupBy(p => p.GetLevel)
                             .ToDictionary(g => g.Key, g => g.ToList());
            if (byLevel.Count == 0) return;

            int minLevel = byLevel.Keys.Min();
            Func<int, int> CalcY = lvl => ConsistentRound((lvl - minLevel) * vGap);

            // =====================================================================
            // (2) Calcular anchos recursivamente
            // =====================================================================
            var widths = new Dictionary<Person, float>();
            var visiting = new HashSet<Person>();

            float ComputeWidth(Person node)
            {
                if (node == null) return hGap;
                if (widths.ContainsKey(node)) return widths[node];
                if (visiting.Contains(node)) { widths[node] = hGap; return hGap; }

                visiting.Add(node);
                var children = adyacencia.ContainsKey(node) ? adyacencia[node] : new List<Person>();

                if (children.Count == 0)
                {
                    float baseW = (node.Partner != null) ? 2f * hGap : 1f * hGap;
                    widths[node] = baseW;
                    if (node.Partner != null) widths[node.Partner] = baseW;
                    visiting.Remove(node);
                    return baseW;
                }

                float total = 0f;
                foreach (var ch in children)
                {
                    float wch = ComputeWidth(ch);
                    total += Math.Max(wch, (ch.Partner != null) ? 2f * hGap : hGap);
                }
                total += hGap * Math.Max(0, children.Count - 1);

                widths[node] = total;
                if (node.Partner != null) widths[node.Partner] = total;

                visiting.Remove(node);
                return total;
            }

            var roots = compMembers.Keys; // usar representantes como raíces
            foreach (var r in roots) ComputeWidth(r);
            foreach (var p in adyacencia.Keys)
                if (!widths.ContainsKey(p)) ComputeWidth(p);

            // =====================================================================
            // (3) Posicionar subárboles top-down
            // =====================================================================
            var placed = new HashSet<Person>();
            float cursorX = 0f;

            void MoveSubtree(Person node, float deltaX)
            {
                if (node == null || deltaX == 0f) return;
                var stack = new Stack<Person>();
                var seen = new HashSet<Person>();
                stack.Push(node);
                while (stack.Count > 0)
                {
                    var cur = stack.Pop();
                    if (cur == null || seen.Contains(cur)) continue;
                    seen.Add(cur);
                    cur.x = ConsistentRound(cur.x + deltaX);
                    if (cur.Partner != null && !seen.Contains(cur.Partner))
                    {
                        cur.Partner.x = ConsistentRound(cur.Partner.x + deltaX);
                        seen.Add(cur.Partner);
                    }
                    if (adyacencia.ContainsKey(cur))
                    {
                        foreach (var ch in adyacencia[cur])
                            if (ch != null && !seen.Contains(ch)) stack.Push(ch);
                    }
                }
            }

            void PlaceSubtree(Person node, float left)
            {
                if (node == null || placed.Contains(node)) return;

                float nodeWidth = widths.ContainsKey(node) ? widths[node] : hGap;
                int lvl = node.GetLevel;
                float y = CalcY(lvl);

                var children = adyacencia.ContainsKey(node) ? adyacencia[node] : new List<Person>();
                var childSlots = new List<float>();
                foreach (var ch in children)
                {
                    float wch = widths.ContainsKey(ch) ? widths[ch] : hGap;
                    childSlots.Add(Math.Max(wch, (ch.Partner != null) ? 2f * hGap : hGap));
                }

                float cLeft = left;
                for (int i = 0; i < children.Count; i++)
                {
                    var ch = children[i];
                    float slot = childSlots[i];
                    float wch = widths.ContainsKey(ch) ? widths[ch] : hGap;
                    float childLeft = cLeft + (slot - wch) / 2f;
                    PlaceSubtree(ch, childLeft);
                    cLeft += slot + hGap;
                }

                float center;
                if (children.Count == 0)
                    center = left + nodeWidth / 2f;
                else if (children.Count == 1)
                    center = children[0].x;
                else
                {
                    float minX = children.Min(ch => ch.x);
                    float maxX = children.Max(ch => ch.x);
                    center = (minX + maxX) / 2f;
                }

                if (node.Partner != null && !placed.Contains(node.Partner))
                {
                    float sep = coupleGap;
                    node.x = ConsistentRound(center - sep / 2f);
                    node.Partner.x = ConsistentRound(center + sep / 2f);
                    node.y = node.Partner.y = ConsistentRound(y);
                    placed.Add(node);
                    placed.Add(node.Partner);
                }
                else
                {
                    node.x = ConsistentRound(center);
                    node.y = ConsistentRound(y);
                    placed.Add(node);
                }
            }

            foreach (var rep in roots)
            {
                float w = widths.ContainsKey(rep) ? widths[rep] : hGap;
                PlaceSubtree(rep, cursorX);
                cursorX += w + hGap;
            }

            // =====================================================================
            // (4) Anti-colisión por nivel
            // =====================================================================
            int diameter = radius * 2;
            float minGap = diameter + Math.Max(10, hGap / 4f);
            var nodesByLevel = adyacencia.Keys.GroupBy(p => p.GetLevel)
                .ToDictionary(g => g.Key, g => g.OrderBy(n => n.x).ToList());

            foreach (var lvl in nodesByLevel.Keys.OrderBy(k => k))
            {
                var list = nodesByLevel[lvl];
                for (int i = 1; i < list.Count; i++)
                {
                    var prev = list[i - 1];
                    var cur = list[i];
                    float prevRight = prev.x + diameter + minGap;
                    if (cur.x < prevRight)
                    {
                        float delta = prevRight - cur.x;
                        MoveSubtree(cur, delta);
                        list = nodesByLevel[lvl].OrderBy(n => n.x).ToList();
                        i = 0;
                    }
                }
            }
        }

        // =====================================================================
        // Debug helper: imprime componentes (parejas) y sus niveles
        // =====================================================================
        private void DebugPrintComponents(Dictionary<Person, List<Person>> compMembers,
                                         Dictionary<Person, int> compIndex,
                                         int[] compLevel)
        {
            Debug.WriteLine("======= COMPONENTES Y NIVELES =======");
            foreach (var kv in compMembers)
            {
                int idx = compIndex[kv.Key];
                int lvl = compLevel[idx];
                string members = string.Join(", ", kv.Value.Select(m => m.GetName));
                Debug.WriteLine($"Nivel {lvl}: [{members}]");
            }
            Debug.WriteLine("====================================");
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

        private void RebuildAdjacency()
        {
            // Recolectar todos los nodos que aún existen (empezamos con las claves actuales)
            var all = new HashSet<Person>(adyacencia.Keys);

            // Añadir nodos referenciados desde children/parents/partner para no perderlos
            foreach (var p in adyacencia.Keys.ToList())
            {
                if (p.Children != null)
                {
                    foreach (var c in p.Children)
                    {
                        if (c != null) all.Add(c);
                    }
                }

                if (p.Parents != null)
                {
                    foreach (var par in p.Parents)
                    {
                        if (par != null) all.Add(par);
                    }
                }

                if (p.Partner != null)
                {
                    all.Add(p.Partner);
                }
            }

            // Construir nuevo diccionario vacío
            var nueva = new Dictionary<Person, List<Person>>();
            foreach (var p in all)
            {
                if (!nueva.ContainsKey(p))
                    nueva[p] = new List<Person>();
            }

            // Poblar con las listas Children actuales (filtrando nulls y asegurando claves)
            foreach (var p in all)
            {
                if (p.Children == null) continue;
                foreach (var c in p.Children)
                {
                    if (c == null) continue;
                    // Asegurarnos de que la clave exista (ya lo hacemos arriba)
                    if (!nueva.ContainsKey(p)) nueva[p] = new List<Person>();
                    if (!nueva[p].Contains(c))
                        nueva[p].Add(c);
                }
            }

            // Reemplazar la estructura
            adyacencia = nueva;
        }

        public void PrintChildren(Person p)
        {
            if (p == null) return;
            if (adyacencia.ContainsKey(p))
            {
                var hijos = adyacencia[p];
                if (hijos != null && hijos.Count > 0)
                {
                    string nombresHijos = string.Join(", ", hijos.ConvertAll(h => h.GetName));
                    Debug.WriteLine($"Padre: {p.GetName} → Hijos: [{nombresHijos}]");
                }
                else
                {
                    Debug.WriteLine($"Padre: {p.GetName} → (sin hijos)");
                }
            }
            else
            {
                Debug.WriteLine($"Padre: {p.GetName} → (no está en adyacencia)");
            }
        }
    }
}
