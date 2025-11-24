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
        private bool nodoInicialCreado = false;
        private Dictionary<Person, List<Person>> adyacencia;
        private int horizontalSpacing;
        private int verticalSpacing;
        private int radius; // Radio de los nodos circulares
        private float minGap = 60f;
        public GrafoGenealogico()
        {
            adyacencia = new Dictionary<Person, List<Person>>();
            horizontalSpacing = 100;
            verticalSpacing = 175;
            radius = 30;
        }
        public void CrearNodoInicial()
        {
            if (!nodoInicialCreado && (adyacencia == null || adyacencia.Count == 0))
            {
                Person p = new Person("N/A", "0000", DateTime.Now, "");

                // Agregar DIRECTAMENTE sin llamar CalculatePositions()
                if (!adyacencia.ContainsKey(p))
                    adyacencia[p] = new List<Person>();

                nodoInicialCreado = true;

                // Calcular posiciones SOLO UNA VEZ al final
                CalculatePositions();
            }
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
        public void AddParent(Person child, Person father)
        {
            if (child == null || father == null) return;

            // ELIMINAR esta parte que causa problemas ↓
            // if (child.Partner != null)
            // {
            //     father.EsSubArbol = true;
            //     father.RaizSubArbol = child;
            // }

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
            try
            {
                // VALIDACIÓN INICIAL CRÍTICA
                if (adyacencia == null)
                    adyacencia = new Dictionary<Person, List<Person>>();

                if (adyacencia.Count == 0)
                    return;

                // Limpiar coordenadas inválidas
                foreach (var persona in adyacencia.Keys.ToList())
                {
                    if (persona == null)
                    {
                        adyacencia.Remove(persona);
                        continue;
                    }

                    if (float.IsNaN(persona.x) || float.IsInfinity(persona.x))
                        persona.x = 0;
                    if (float.IsNaN(persona.y) || float.IsInfinity(persona.y))
                        persona.y = 0;

                    // ELIMINAR flags de sub-árbol
                    persona.EsSubArbol = false;
                    persona.RaizSubArbol = null;
                }

                int ConsistentRound(float value) => (int)Math.Floor(value + 0.5f);

                float hGap = horizontalSpacing;
                float vGap = verticalSpacing;
                float coupleGap = Math.Max(hGap * 1.5f, 120f);

                // =====================================================================
                // (0) Recalcular niveles globales - MANTENER ESTO
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

                // --- Construir componentes ---
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

                // =====================================================================
                // (1) POSICIONAMIENTO SIMPLE - TODOS EN EL MISMO ÁRBOL
                // =====================================================================

                // Agrupar por nivel
                var byLevel = allNodes.GroupBy(p => p.GetLevel)
                                     .OrderBy(g => g.Key)
                                     .ToDictionary(g => g.Key, g => g.ToList());

                if (byLevel.Count == 0) return;

                int minLevel = byLevel.Keys.Min();
                Func<int, int> CalcY = lvl => ConsistentRound((lvl - minLevel) * vGap);

                // Calcular anchos por nivel
                var levelWidths = new Dictionary<int, float>();
                foreach (var levelGroup in byLevel)
                {
                    int level = levelGroup.Key;
                    var persons = levelGroup.Value;

                    // Calcular ancho necesario para este nivel
                    float levelWidth = 0f;
                    foreach (var person in persons)
                    {
                        if (person.Partner != null && persons.Contains(person.Partner))
                        {
                            // Pareja ocupa espacio doble
                            levelWidth += coupleGap + hGap;
                        }
                        else
                        {
                            levelWidth += hGap * 2;
                        }
                    }
                    levelWidths[level] = Math.Max(levelWidth, hGap * 3);
                }

                // Encontrar el ancho máximo para centrar todos los niveles
                float maxLevelWidth = levelWidths.Values.Max();

                // Posicionar cada nivel
                foreach (var levelGroup in byLevel)
                {
                    int level = levelGroup.Key;
                    var persons = levelGroup.Value;
                    float y = CalcY(level);

                    // Calcular punto de inicio para centrar este nivel
                    float levelWidth = levelWidths[level];
                    float startX = (maxLevelWidth - levelWidth) / 2f;

                    float currentX = startX;

                    // Ordenar personas para poner parejas juntas
                    var sortedPersons = new List<Person>();
                    var processed = new HashSet<Person>();
                    var personLevel = persons.ToDictionary(p => p, p => 0); // 0 = izquierda, 1 = derecha

                    foreach (var person in persons)
                    {
                        if (processed.Contains(person)) continue;

                        if (person.Partner != null && persons.Contains(person.Partner))
                        {
                            // DECIDIR en qué lado poner a cada uno de la pareja
                            int sidePerson = DeterminarLadoOptimo(person, persons);
                            int sidePartner = (sidePerson == 0) ? 1 : 0;

                            personLevel[person] = sidePerson;
                            personLevel[person.Partner] = sidePartner;

                            // Agregar en el orden correcto
                            if (sidePerson == 0) // persona a la izquierda
                            {
                                sortedPersons.Add(person);
                                sortedPersons.Add(person.Partner);
                            }
                            else // persona a la derecha
                            {
                                sortedPersons.Add(person.Partner);
                                sortedPersons.Add(person);
                            }

                            processed.Add(person);
                            processed.Add(person.Partner);
                        }
                        else
                        {
                            sortedPersons.Add(person);
                            processed.Add(person);
                        }
                    }

                    // Asignar posiciones
                    foreach (var person in sortedPersons)
                    {
                        if (person.Partner != null && sortedPersons.Contains(person.Partner))
                        {
                            // Posicionar pareja
                            person.x = ConsistentRound(currentX);
                            person.Partner.x = ConsistentRound(currentX + coupleGap);
                            person.y = person.Partner.y = ConsistentRound(y);
                            currentX += coupleGap + hGap;
                        }
                        else
                        {
                            // Persona individual
                            person.x = ConsistentRound(currentX);
                            person.y = ConsistentRound(y);
                            currentX += hGap * 2;
                        }
                    }
                }

                // =====================================================================
                // (2) ANTI-COLISIÓN MEJORADA
                // =====================================================================
                var nodesByLevel = allNodes
                    .GroupBy(p => p.GetLevel)
                    .ToDictionary(g => g.Key, g => g.OrderBy(n => n.x).ToList());

                foreach (var lvl in nodesByLevel.Keys.OrderBy(k => k))
                {
                    var list = nodesByLevel[lvl];
                    bool moved;
                    int safety = 0;

                    do
                    {
                        moved = false;
                        for (int i = 1; i < list.Count && safety < 50; i++)
                        {
                            var prev = list[i - 1];
                            var cur = list[i];

                            float prevRight = prev.x + (radius * 2) + minGap;
                            if (cur.x < prevRight)
                            {
                                float delta = prevRight - cur.x;

                                // Mover todos los nodos a la derecha
                                for (int j = i; j < list.Count; j++)
                                {
                                    list[j].x += delta;
                                }

                                moved = true;
                                safety++;
                                break;
                            }
                        }
                    } while (moved && safety < 50);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR en CalculatePositions: {ex.Message}");
                PosicionamientoSimpleFallback();
            }
        }
        private void PosicionamientoSimpleFallback()
        {
            int x = 50;
            int y = 50;
            foreach (var persona in adyacencia.Keys)
            {
                if (persona == null) continue;
                persona.x = x;
                persona.y = y;
                x += horizontalSpacing;
                if (x > 800) { x = 50; y += verticalSpacing; }
            }
        }
        // =====================================================================
        // Métodos auxiliares para sub-árboles
        // =====================================================================

        private HashSet<Person> ColectarPersonasSubArbol(Person raiz)
        {
            var personas = new HashSet<Person>();
            var stack = new Stack<Person>();
            stack.Push(raiz);

            while (stack.Count > 0)
            {
                var actual = stack.Pop();
                if (actual == null || personas.Contains(actual)) continue;

                personas.Add(actual);

                // Agregar padres
                if (actual.Parents != null)
                {
                    foreach (var padre in actual.Parents)
                        if (padre != null) stack.Push(padre);
                }

                // Agregar hijos
                if (actual.Children != null)
                {
                    foreach (var hijo in actual.Children)
                        if (hijo != null) stack.Push(hijo);
                }

                // Agregar pareja
                if (actual.Partner != null) stack.Push(actual.Partner);
            }

            return personas;
        }

        private float PosicionarSubArbolIndependiente(Person raiz, float xBase, float yBase, float hGap, float vGap)
        {
            var personasSubArbol = ColectarPersonasSubArbol(raiz);


            // Calcular niveles relativos dentro del sub-árbol
            var niveles = personasSubArbol.GroupBy(p => p.GetLevel).OrderBy(g => g.Key).ToList();
            int nivelMinimo = niveles.Min(g => g.Key);

            // Posicionar cada persona del sub-árbol
            float maxAncho = 0f;

            foreach (var nivelGroup in niveles)
            {
                int nivelRelativo = nivelGroup.Key - nivelMinimo;
                float y = yBase + (nivelRelativo * vGap);
                var personasNivel = nivelGroup.ToList();

                // Distribuir horizontalmente
                float anchoNivel = personasNivel.Count * (hGap * 2);
                float xInicio = xBase;

                for (int i = 0; i < personasNivel.Count; i++)
                {
                    var persona = personasNivel[i];
                    persona.x = xInicio + (i * hGap * 2);
                    persona.y = y;

                    // Posicionar pareja al lado si existe
                    if (persona.Partner != null && personasSubArbol.Contains(persona.Partner))
                    {
                        persona.Partner.x = persona.x + hGap;
                        persona.Partner.y = y;
                        i++; // Saltar el siguiente slot
                    }
                }

                maxAncho = Math.Max(maxAncho, anchoNivel);
            }

            return maxAncho;
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
            Pen linea = new Pen(Color.Black, 3); // Línea más delgada para mejor visibilidad
            Pen lineaPareja = new Pen(Color.Blue, 2);
            Pen lineaSubArbol = new Pen(Color.DarkGreen, 3);

            int diameter = radius * 2;

            // === 1️⃣ VERIFICAR Y LIMPIAR COORDENADAS INVÁLIDAS ===
            foreach (var persona in adyacencia.Keys)
            {
                if (float.IsNaN(persona.x) || float.IsInfinity(persona.x) ||
                    float.IsNaN(persona.y) || float.IsInfinity(persona.y))
                {
                    persona.x = 0;
                    persona.y = 0;
                }
            }

            // === 2️⃣ SOLO UN SISTEMA DE CONEXIONES - LÍNEAS DIRECTAS ===
            foreach (var kvp in adyacencia)
            {
                var padre = kvp.Key;
                var hijos = kvp.Value;

                if (padre.EsSubArbol) continue;
                if (hijos == null || hijos.Count == 0) continue;

                var hijosPrincipal = hijos.Where(h => h != null && !h.EsSubArbol &&
                    IsValidCoordinate(h.x, h.y)).ToList();

                if (hijosPrincipal.Count == 0) continue;

                // VERIFICAR COORDENADAS DEL PADRE
                if (!IsValidCoordinate(padre.x, padre.y)) continue;

                // Para cada hijo, dibujar línea DIRECTA del padre al hijo
                foreach (var hijo in hijosPrincipal)
                {
                    try
                    {
                        float xPadre, yPadre;

                        // Si tiene pareja, usar punto medio entre la pareja
                        if (padre.Partner != null && !padre.Partner.EsSubArbol &&
                            IsValidCoordinate(padre.Partner.x, padre.Partner.y))
                        {
                            xPadre = (padre.x + padre.Partner.x) / 2f + radius;
                            yPadre = padre.y + radius;
                        }
                        else
                        {
                            xPadre = padre.x + radius;
                            yPadre = padre.y + radius;
                        }

                        float xHijo = hijo.x + radius;
                        float yHijo = hijo.y + radius;

                        // Usar línea curva para mejor apariencia
                        DibujarLineaCurva(g, linea, xPadre, yPadre, xHijo, yHijo);

                        // Opcional: dibujar flecha para indicar dirección
                        // DibujarFlecha(g, linea, xPadre, yPadre, xHijo, yHijo);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error dibujando línea padre-hijo: {ex.Message}");
                    }
                }
            }

            // === 3️⃣ Dibujar líneas entre parejas ===
            foreach (var kvp in adyacencia)
            {
                var persona = kvp.Key;

                if (persona.EsSubArbol) continue;

                if (persona.Partner != null && !persona.Partner.EsSubArbol)
                {
                    // VERIFICAR COORDENADAS ANTES DE DIBUJAR
                    if (IsValidCoordinate(persona.x, persona.y) &&
                        IsValidCoordinate(persona.Partner.x, persona.Partner.y))
                    {
                        float x1 = persona.x + radius;
                        float y1 = persona.y + radius;
                        float x2 = persona.Partner.x + radius;
                        float y2 = persona.Partner.y + radius;

                        // Verificar que los puntos sean distintos y estén en rangos razonables
                        if (Math.Abs(x1 - x2) < 1000 && Math.Abs(y1 - y2) < 1000)
                        {
                            try
                            {
                                g.DrawLine(lineaPareja, x1, y1, x2, y2);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error dibujando línea de pareja: {ex.Message}");
                            }
                        }
                    }
                }
            }

            // === 4️⃣ Dibujar los NODOS ===
            foreach (var kvp in adyacencia)
            {
                var p = kvp.Key;

                // VERIFICAR COORDENADAS ANTES DE DIBUJAR
                if (!IsValidCoordinate(p.x, p.y)) continue;

                Brush colorNodo = Brushes.LightBlue;

                try
                {
                    // Círculo
                    g.FillEllipse(colorNodo, p.x, p.y, diameter, diameter);
                    g.DrawEllipse(Pens.Black, p.x, p.y, diameter, diameter);

                    // Nombre
                    string nombreMostrar = p.GetName.Length > 10 ?
                        p.GetName.Substring(0, 8) + "..." : p.GetName;

                    var textSize = g.MeasureString(nombreMostrar, font);
                    float textX = p.x + radius - textSize.Width / 2;
                    float textY = p.y + radius - textSize.Height / 2;
                    g.DrawString(nombreMostrar, font, texto, textX, textY);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error dibujando nodo {p.GetName}: {ex.Message}");
                }
            }
        }

        // Método auxiliar para verificar coordenadas
        private bool IsValidCoordinate(float x, float y)
        {
            return !float.IsNaN(x) && !float.IsInfinity(x) &&
                   !float.IsNaN(y) && !float.IsInfinity(y) &&
                   Math.Abs(x) < 10000 && Math.Abs(y) < 10000;
        }
        private void DibujarFlecha(Graphics g, Pen pen, float x1, float y1, float x2, float y2)
        {
            // Calcular punto medio
            float mx = (x1 + x2) / 2;
            float my = (y1 + y2) / 2;

            // Calcular ángulo de la línea
            double angle = Math.Atan2(y2 - y1, x2 - x1);

            // Tamaño de la flecha
            float arrowSize = 8;

            // Puntos de la flecha
            PointF[] arrowPoints = new PointF[3];

            // Punta de la flecha
            arrowPoints[0] = new PointF(mx, my);

            // Base de la flecha
            arrowPoints[1] = new PointF(
                (float)(mx - arrowSize * Math.Cos(angle - Math.PI / 6)),
                (float)(my - arrowSize * Math.Sin(angle - Math.PI / 6))
            );

            arrowPoints[2] = new PointF(
                (float)(mx - arrowSize * Math.Cos(angle + Math.PI / 6)),
                (float)(my - arrowSize * Math.Sin(angle + Math.PI / 6))
            );

            // Dibujar flecha
            g.FillPolygon(Brushes.Gray, arrowPoints);
        }
        // =====================================================================
        // Métodos auxiliares para dibujar sub-árboles
        // =====================================================================
        private void DibujarSubArbolCompleto(Graphics g, Person raizSubArbol, Pen lineaSubArbol, Pen lineaPareja)
        {
            var personasSubArbol = ColectarPersonasSubArbol(raizSubArbol);

            // === A. Dibujar líneas entre parejas en el sub-árbol ===
            foreach (var persona in personasSubArbol)
            {
                if (persona.Partner != null && personasSubArbol.Contains(persona.Partner))
                {
                    float x1 = persona.x + radius;
                    float y1 = persona.y + radius;
                    float x2 = persona.Partner.x + radius;
                    float y2 = persona.Partner.y + radius;

                    g.DrawLine(lineaPareja, x1, y1, x2, y2);
                }
            }

            // === B. Dibujar conexiones padres → hijos en el sub-árbol ===
            foreach (var persona in personasSubArbol)
            {
                if (persona.Children == null || persona.Children.Count == 0)
                    continue;

                // Filtrar hijos que están en el mismo sub-árbol
                var hijosSubArbol = persona.Children.Where(h => h != null && personasSubArbol.Contains(h)).ToList();
                if (hijosSubArbol.Count == 0) continue;

                // Punto de salida
                float xOrigen, yOrigen;

                if (persona.Partner != null && personasSubArbol.Contains(persona.Partner))
                {
                    xOrigen = (persona.x + persona.Partner.x) / 2f + radius;
                    yOrigen = persona.y + radius;
                }
                else
                {
                    xOrigen = persona.x + radius;
                    yOrigen = persona.y + radius;
                }

                // Dibujar conexiones
                if (hijosSubArbol.Count > 0)
                {
                    float yConexion = hijosSubArbol[0].y - 20;
                    g.DrawLine(lineaSubArbol, xOrigen, yOrigen, xOrigen, yConexion);

                    // Línea horizontal
                    float minX = hijosSubArbol.Min(h => h.x + radius);
                    float maxX = hijosSubArbol.Max(h => h.x + radius);
                    g.DrawLine(lineaSubArbol, minX, yConexion, maxX, yConexion);

                    // Líneas verticales a cada hijo
                    foreach (var h in hijosSubArbol)
                    {
                        float xHijo = h.x + radius;
                        float yHijo = h.y;
                        g.DrawLine(lineaSubArbol, xHijo, yConexion, xHijo, yHijo);
                    }
                }
            }
        }
        private int DeterminarLadoOptimo(Person persona, List<Person> personasNivel)
        {
            // ESTRATEGIA MEJORADA: Analizar toda la estructura familiar

            // 1. Si tiene pareja y la pareja ya está posicionada, usar el lado opuesto
            if (persona.Partner != null && persona.Partner.x != 0)
            {
                return (persona.Partner.x < persona.x) ? 1 : 0;
            }

            // 2. Analizar la posición de los padres (si existen)
            float promedioPadresX = 0;
            int padresCount = 0;

            if (persona.Parents != null)
            {
                foreach (var padre in persona.Parents)
                {
                    if (padre != null && padre.x != 0)
                    {
                        promedioPadresX += padre.x;
                        padresCount++;
                    }
                }
            }

            if (padresCount > 0)
            {
                promedioPadresX /= padresCount;
                // Si los padres están principalmente a la izquierda, poner a la persona a la derecha
                return (promedioPadresX < 400) ? 1 : 0;
            }

            // 3. Analizar la posición de los hijos (si existen)
            float promedioHijosX = 0;
            int hijosCount = 0;

            if (persona.Children != null && persona.Children.Count > 0)
            {
                foreach (var hijo in persona.Children)
                {
                    if (hijo != null && hijo.x != 0)
                    {
                        promedioHijosX += hijo.x;
                        hijosCount++;
                    }
                }
            }

            if (hijosCount > 0)
            {
                promedioHijosX /= hijosCount;
                // Si los hijos están principalmente a la izquierda, poner a la persona a la derecha
                return (promedioHijosX < 400) ? 1 : 0;
            }

            // 4. Estrategia de balanceo por nivel
            int personasEnNivel = personasNivel.Count;
            int indicePersona = personasNivel.IndexOf(persona);

            // Alternar lados para balancear el nivel
            return (indicePersona % 2 == 0) ? 0 : 1;
        }
        private void DibujarLineaCurva(Graphics g, Pen pen, float x1, float y1, float x2, float y2)
        {
            try
            {
                // LÍNEAS MÁS RECTAS - menos curvas
                float controlX1, controlY1, controlX2, controlY2;

                float distanciaX = Math.Abs(x1 - x2);
                float distanciaY = Math.Abs(y1 - y2);

                if (distanciaX > distanciaY)
                {
                    // Conexión más horizontal - curva MUY suave
                    float factorCurva = 0.1f; // Reducido de 0.3f a 0.1f para menos curva
                    controlX1 = x1 + (x2 - x1) * factorCurva;
                    controlY1 = y1;
                    controlX2 = x2 - (x2 - x1) * factorCurva;
                    controlY2 = y2;
                }
                else
                {
                    // Conexión más vertical - curva MUY suave
                    float factorCurva = 0.1f; // Reducido de 0.3f a 0.1f para menos curva
                    controlX1 = x1;
                    controlY1 = y1 + (y2 - y1) * factorCurva;
                    controlX2 = x2;
                    controlY2 = y2 - (y2 - y1) * factorCurva;
                }

                g.DrawBezier(pen, x1, y1, controlX1, controlY1, controlX2, controlY2, x2, y2);
            }
            catch
            {
                // Fallback: línea recta
                g.DrawLine(pen, x1, y1, x2, y2);
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

