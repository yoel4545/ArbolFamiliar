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
        private float horizontalSpacing;
        private float verticalSpacing;
        private float coupleSpacing;
        private int radius; // Radio de los nodos circulares
        public GrafoGenealogico()
        {
            adyacencia = new Dictionary<Person, List<Person>>();
            horizontalSpacing = 250;
            verticalSpacing = 175;
            radius = 30;
            coupleSpacing = horizontalSpacing/2;
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
            CalculatePositionsAddChildren(father, child);
        }

        private void CalculatePositionsAddChildren(Person father, Person child)
        {
            double center;
            if (father.HasPartner())
            {
                center = (father.x + father.Partner.x) / 2;
            }
            else
            {
                center = father.x;
            }
            child.y = father.y + verticalSpacing;
            foreach (Person p in father.Children)
            {
                if (p != child)
                {
                    p.x -= horizontalSpacing / 2;
                    if (p.Partner != null)
                    {
                        p.Partner.x -= horizontalSpacing / 2;
                    }
                    PropagateMoveDescendants(p, -horizontalSpacing / 2);
                }
            }
            child.x = father.GetRightMostChildX() + horizontalSpacing;
            if (father.Children.Count ==1) 
                child.x = (float)center;
        }

        private void PropagateMoveDescendants(Person p, float deltaX) //Mueve los descendientes de p horizontalmente
        {
            foreach (var child in p.Children)
            {
                child.x += deltaX;
                if (child.Partner != null)
                {
                    PropageteMoveAscendants(child.Partner, deltaX);
                }
                PropagateMoveDescendants(child, deltaX);
            }
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
            CalculatePositionsAddParent(child, father);
        }

        public void CalculatePositionsAddParent(Person child, Person father)
        {
            if (child.Partner != null)
            {
                if (child.x > child.Partner.x)
                {
                    child.x = child.Partner.RightMostParent() + horizontalSpacing;
                    PropagateMoveDescendants(child, horizontalSpacing);
                }
            }
            if (child.CountParents() == 1)
            {
                father.x = child.x;
                father.y = child.y - verticalSpacing;
            }
            else if (child.CountParents() == 2)
            {
                var otherParent = child.Parents.FirstOrDefault(p => p != father);
                father.x = otherParent.x + horizontalSpacing;
                father.y = otherParent.y;
                PropagateMoveDescendants(father, horizontalSpacing / 2);
            }
        }

        private void PropageteMoveAscendants(Person p, float deltaX) //Mueve los ascendentes (hermanos tambien) de p horizontalmente
        {
            bool movedBrothers = false;
            if (p ==null) return;
            if (p.Parents[0] != null)
            {
                Debug.WriteLine("Moviendo ascendente: " + p.Parents[0].GetName);
                p.Parents[0].x += deltaX;
                if (!movedBrothers)
                {
                    foreach (var sibling in p.Parents[0].Children)
                    {
                        sibling.x += deltaX;
                    }
                }
                movedBrothers = true;
                PropageteMoveAscendants(p.Parents[0], deltaX);
            }
            else if (p.Parents[1] != null)
            {
                Debug.WriteLine("Moviendo ascendente: " + p.Parents[1].GetName);
                p.Parents[1].x += deltaX;
                if (!movedBrothers)
                {
                    foreach (var sibling in p.Parents[1].Children)
                    {
                        sibling.x += deltaX;
                    }
                }
                movedBrothers = true;
                PropageteMoveAscendants(p.Parents[1], deltaX);
            }
            
        }

        public void AddPartner(Person existingPatner, Person newPatner)
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
            CalculatePositionsAddPartner(existingPatner, newPatner);
        }

        private void CalculatePositionsAddPartner(Person existingPatner, Person newPatner)
        {
            newPatner.x = existingPatner.x + coupleSpacing;
            newPatner.y = existingPatner.y;
            PropagateMoveDescendants(existingPatner, coupleSpacing/2);
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
