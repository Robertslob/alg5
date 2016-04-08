using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace algo5
{
    class Programm
    {
        static void Main(string[] args)
        {
            string s = Console.ReadLine();
            string[] strings = s.Split(' ');
            ushort n = ushort.Parse(strings[0]);
            ushort m = ushort.Parse(strings[1]);
            ushort modus = ushort.Parse(strings[2]);
            int[,] matrixIn = new int[m,n];
            int[] rowSum = new int[m];            
            int[] columnSum = new int[n];
            int[] exRow = new int[m];
            int[] exCol = new int[n];

            for (int i = 0; i < m; i++)
            {
                s = Console.ReadLine();
                strings = s.Split(' ');
                int sum = 0;
                for (int j = 0; j < n; j++)
                {
                    matrixIn[i, j] = int.Parse(strings[j]);
                    sum += matrixIn[i,j];
                    columnSum[j] += matrixIn[i,j];
                }
                exRow[i] = int.Parse(strings[n]);
                rowSum[i] = int.Parse(strings[n]) - sum;
            }
            s = Console.ReadLine();
            strings = s.Split(' ');
            for (int j = 0; j < n; j++)
            {
                exCol[j] = int.Parse(strings[j]);
                columnSum[j] -= int.Parse(strings[j]);
                columnSum[j] *= -1;
            }

            // Rowsum en Columnsum hebben gewenste waarde min huidige waarde --> kleinere getallen.
            int[] currColumn = new int[n];
            int[] currRow = new int[m];
            int[,] matrixMarge = new int[m, n];
            for (int i = 0; i < m; i++)
            {
                s = Console.ReadLine();
                strings = s.Split(' ');
                for (int j = 0; j < n; j++)
                {
                    matrixMarge[i, j] = int.Parse(strings[j]);
                    columnSum[j] += matrixMarge[i,j];
                    rowSum[i] += matrixMarge[i,j];
                    currColumn[j] += matrixMarge[i, j];
                    currRow[i] += matrixMarge[i, j];
                    matrixMarge[i,j] *= 2;
                }
            }

            // Rowsum en columnsum waardes van marge bij opgeteld, dus marge nu kiezen tussen 0...2*marge, dus geen negatieve waardes meer.

            // ?? aangezien we geen negatieve waardes meer kunnen kiezen, betekent dit dat als rowsum of columnsum < 0, dan niet mogelijk.
            bool mogelijk = true;
            for(int i = 0; i<n; i++)
            {
                if(columnSum[i] < 0){
                    Console.WriteLine("kan niet");
                    mogelijk = false;
                    break;
                }
            }

            if(mogelijk){
                for(int i = 0; i<m; i++)
                {
                    if(rowSum[i] < 0){
                        Console.WriteLine("kan niet");
                        break;
                    }
                }
            }

            

            // Nu berekenen of het mogelijk is.

            Program p = new Program(n, m, matrixMarge, matrixIn, exRow, exCol, modus);
            p.CreateBronAndSink(columnSum, rowSum);
            p.CompleteNodes();
            p.Calculate();
        
            

        }
    }

    class Program
    {
        int[] expectedRowSum, expectedColumnSum;
        Node sink, bron;
        Node[] rowNodes, columnNodes;
        int flow = 0; // Eigenlijk groter, maar maakt niet uit.
        ushort n,m;
        int[,] marge, matrixIn;
        int modus;
        Node[] nodes;

        public Program(ushort n, ushort m, int[,] matrixMarge, int[,] matrixIn, int[] exRow, int[] exCol, int modus)
        {
            this.n = n;
            this.m = m;
            marge = matrixMarge;
            this.matrixIn = matrixIn;
            rowNodes = new Node[m];
            columnNodes = new Node[n];
            expectedRowSum = exRow;
            expectedColumnSum = exCol;
            this.modus = modus;
            nodes = new Node[n + m + 2];
        }

        // creeert bron en put
        public void CreateBronAndSink(int[] column, int[] row){
            bron = new Node(null, null, 0, 0);
            sink = new Node(null, null, (ushort)(n + m + 1), 3);

            for(int i = 0; i < m; i++)
                rowNodes[i] = new Node(null, null, (ushort)(1+i), 1);
            for(int i = 0; i < n; i++)
                columnNodes[i] = new Node(null, null, (ushort)(1+m+i), 2);
            
            Edge[] bronOuts = new Edge[m];
            for(int i = 0; i< m; i++){
                Edge e = new Edge(bron, rowNodes[i], row[i], 0);
                bronOuts[i] = e;
                Edge ee = new Edge(bron, rowNodes[i], 0, 0);
                rowNodes[i].ins = new Edge[1]{ee};
            }
            Edge[] sinkIns = new Edge[n];
            for(int i = 0; i< n; i++){
                Edge e = new Edge(columnNodes[i], sink, 0, 0);
                sinkIns[i] = e;
                Edge ee = new Edge(columnNodes[i], sink, column[i], 0);
                columnNodes[i].outs = new Edge[1]{ee};
            }
            bron.outs = bronOuts;
            sink.ins = sinkIns;
        }

        // completeert overige nodes, ook de reverse-edges zelfde capaciteit gegeven (???)
        public void CompleteNodes(){
            for(int i = 0; i<m; i++){         
                Edge[] outs = new Edge[n];       
                for(int j = 0; j<n; j++)
                {
                    outs[j] = new Edge(rowNodes[i], columnNodes[j], marge[i,j], 0);//marge[i,j]/2);
                }
                rowNodes[i].outs = outs;               
            }

            // Reverse-edges
            for(int j = 0; j<n; j++){
                Edge[] ins = new Edge[m];
                for(int i = 0; i<m; i++)
                    ins[i] = new Edge(rowNodes[i], columnNodes[j], 0, 0);//-marge[i,j]/2);
                columnNodes[j].ins = ins;                
            }
        }


        int[] parents;
        int topFlow;

        public void BFS()
        {
            // Waardes representeren waardes op je pas, zodat je min efficient kan nemen.
            // Ouders representeren waar je vandaan komt --> een pad is maar 2 variables???? wiki
            // index: bron = 0, sink = n+m+1, 1...m row, m+1...n column.
            int[] ouders = new int[n+m+2];
            for(int i = 0; i< n+m+2; i++)
                ouders[i] = -1;
            ouders[0] = -2; // ????
            int[] waardes = new int[n+m+2];
            waardes[0] = int.MaxValue;
            Queue Q = new Queue();
            Q.Enqueue(bron);
            while (Q.Count > 0)
            {
                Node x = (Node)Q.Dequeue();
                int max = x.outs.Length;
                for(int i = 0; i < max; i++){
                    if (x == bron && i == 5)
                        x = x;
                    Edge e = x.outs[i];
                    Node t = e.target;
                    if(ouders[t.count] == -1 && e.capacity - e.flow > 0){
                        e.target.bezocht = true; // ??
                        // x --> t
                        // 1: x is tussen 0 en m, 2: x is tussen 0 en n, ...
                        ouders[t.count] = x.count;
                        waardes[t.count] = Math.Min(e.capacity - e.flow, waardes[x.count]);
                        if(t != sink)
                            Q.Enqueue(t);
                        else{
                            parents = ouders;
                            topFlow = waardes[sink.count];
                            return;
                        }
                    }
                }
            }
            parents = ouders;
            topFlow = 0;

        }

        public void Calculate(){
            while(true){
                BFS();
                if(topFlow == 0)
                    break;
                flow += topFlow;
                Node v = sink;
                Node u;
                 
                while(v!= bron){
                    int pCount = parents[v.count];
                    switch(v.fase)
                    {
                        case 3:                // sink, dus parent heeft maar 1 out, de out van de parent is                                       
                            u = v.ins[pCount - m - 1].source;
                            u.outs[0].flow += topFlow;
                            v.ins[pCount - m - 1].flow -= topFlow;
                            break;
                        case 2:
                            u = v.ins[pCount - 1].source;
                            u.outs[v.count - m - 1].flow += topFlow;
                            v.ins[pCount - 1].flow -= topFlow;
                            break;
                        default:
                            if (v.fase == 0)
                                v = v;
                            u = bron;
                            u.outs[v.count - 1].flow += topFlow;
                            v.ins[0].flow -= topFlow;
                            break;
                    }
                    v = u;
                }
            }
            outPutTjak();

        }

        void outPutTjak()
        {
            for (int i = 0; i < n; i++)
            {
                int s = 0;
                string ss = "";
                for (int j = 0; j < m; j++)
                {
                    s+=columnNodes[i].ins[j].flow;
                    
                    //ss += columnNodes[j].ins[i].flow + " ";
                }
                ss += columnNodes[i].outs[0].flow + " " + columnNodes[i].outs[0].capacity;
                Console.WriteLine(s);
                Console.WriteLine(ss);
            }
            /*
            bool waarheid = true;
            int[] colloSums = new int[n];
            for (int i = 0; i < m; i++)
            {
                int sum = 0;
                for(int j = 0; j<n; j++){
                    int accentij = matrixIn[i, j] + rowNodes[i].outs[j].flow - marge[i, j] / 2;
                    colloSums[j] += accentij;
                    sum += accentij;
                }
                if(sum != expectedRowSum[i]){
                    waarheid = false;
                    break;
                }
            }

            for(int i = 0; i< n; i++){
                if(colloSums[i] != expectedColumnSum[i]){
                    waarheid = false;
                    break;
                }
            }

            if(true){
                if(modus == 2){
                    Console.WriteLine("kan wel");
                }
                else{
                    for (int i = 0; i < m; i++)
                    {
                        string s = "";
                        for (int j = 0; j < n; j++)
                        {
                            int accentij = matrixIn[i, j] + rowNodes[i].outs[j].flow - marge[i, j] / 2;
                            s += accentij + " ";
                        }
                        s += expectedRowSum[i];
                        Console.WriteLine(s);
                    }
                    string str = "";
                    for (int i = 0; i < n-1; i++)
                    {
                        str += expectedColumnSum[i]+ " ";
                    }
                    str += expectedColumnSum[n-1];
                    Console.WriteLine(str);
                }
                
            }
            else Console.WriteLine("kan niet");           
            */
            Console.ReadLine();
        }
    }

    class Node
    {
        public Edge[] ins, outs;
        public bool bezocht;
        public ushort count; // plek in array n+m+2
        public ushort fase; // 0, 1, 2 of 3, voor plek in netwerk
        public Node(Edge[] ins, Edge[] outs, ushort c, ushort f)
        {
            this.ins = ins;
            this.outs = outs;
            bezocht = false;
            count = c;
            fase = f;
        }

    }

    class Edge{
        
        public int flow, capacity;
        public Node source, target;
        public Edge(Node s, Node t, int c, int f){
            source = s;
            target = t;
            capacity = c;
            flow = f;
        }
    }
}
