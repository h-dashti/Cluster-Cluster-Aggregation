/*
  In this simulation we consider N particles. Each of particles are a cluster with unit mass one. The particle doe random walks until
  one of them become a nearest neighbour of another particle. At that point they stick together and form a cluster of two particles.
  The new cluster now moves with as a single random walker with a reduced diffusion coefficient. So the cluster become larger and
  fewer in number. We consider a square latice with periodic conditions. The CCA algorithm can be summarized as follows:
  1. Place N particles at random positions on the lattice. Then identify i_th particle with i_th cluster
  2. Ceck if any two clusters have partciles that are nearset neighours. If so join these two clusters to form a single cluster.
  3. Choose a cluster at random and move it (we have several condition for motions )
  4. Repeat Step 2 and 3 until desired time or number of cluster is one.
  
 VARIABLES:
  array x[], y[] :         the position of each particles
  array site[,]  :         is zero if there is no partilce in (x,y) else equal the lable of the cluster that exist in postiion (x,y)
  array firstParticle[k]:  gives the particle lable of the first particle in k_th cluster
  array nextParticle[i] :  equals the particle lable of another particle in same cluster
*/


using System;

namespace CCA
{
    public class CCA
    {
        public int N; // number of particles
        public int nClusters;
        public int[,] site;
        public int L;
        public int[] mass;
        public int massMin;  // Ds ~ s^(-1/d) that d = 2
       

        int[] firstParticle, nextParticle;
        int[] x, y;
        int[] nX = { 1, 0, -1, 0 };
        int[] nY = { 0, 1, 0, -1 };
        Random rand = new Random();



        public CCA(int N, int L)
        {
            this.N = N;
            this.L = L;
            site = new int[L, L];
            firstParticle = new int[N + 1];
            nextParticle = new int[N + 1];
            x = new int[N + 1];
            y = new int[N + 1];
            mass = new int[N + 1];
            Initialize();
        }

        void Initialize()
        {
            nClusters = 0;
            int xi, yi, i;
            for (i = 1; i <= N; i++)
            {
                do { xi = rand.Next(L); yi = rand.Next(L); } while (site[xi, yi] != 0);
                x[i] = xi;
                y[i] = yi;
                nClusters++;
                site[xi, yi] = nClusters;
                firstParticle[nClusters] = i;
                nextParticle[i] = i;

                mass[nClusters] = 1;

                if (nClusters > 1)
                    Neighbour(xi, yi);
            }
        }


        void Neighbour(int xi, int yi) 
        {
            int nn, px, py, perimeter, part;
            for (nn = 0; nn < 4; nn++)
            {
                px = PBC(xi + nX[nn]);
                py = PBC(yi + nY[nn]);
                perimeter = site[px, py];
                part = site[xi, yi];
                if (perimeter != 0 && perimeter != part)
                    Merge(perimeter, part);
            }
        }
        void Merge(int c1, int c2)
        {
            int p1, p2, p1Next, p2Next;
            p1 = firstParticle[c1];
            p2 = firstParticle[c2];
            p1Next = nextParticle[p1];
            p2Next = nextParticle[p2];

            nextParticle[p1] = p2Next;
            nextParticle[p2] = p1Next;

            int massSum = mass[c1] + mass[c2];

            do
            {
                site[x[p2Next], y[p2Next]] = c1;
                p2Next = nextParticle[p2Next];
            }while(p2Next != p1Next);

            mass[c1] = massSum;

            if (c2 != nClusters) 
            {
                int pLast = firstParticle[nClusters];       // now afther combine two clusters then we replace cluster c2 with the last cluster
                int p = pLast;
                do
                {
                    site[x[p], y[p]] = c2;
                    p = nextParticle[p];
                }while( p != pLast);
                firstParticle[c2] = pLast;
                mass[c2] = mass[nClusters];
            }
            nClusters--;
        }

        void FindMinMass()
        {
            massMin = mass[1];
            for (int n = 2; n < nClusters; n++)
            {
                if (mass[n] < massMin) massMin = mass[n];
            }
        }

        int PBC(int i)
        {
            if (i < 0) return L - 1;
            else if (i > L - 1) return 0;
            else return i;
        }


        public void Move()
        {
            
            int c = rand.Next(1, nClusters + 1); // we choose randomly one cluster

            FindMinMass();
            double Dmax = 1.0 / Math.Sqrt(massMin);
            double Ds = 1.0 / Math.Sqrt(mass[c]);
            double r = rand.NextDouble();
            if (r >= Ds / Dmax) return;         // then the cluster c can not move


            int direction = rand.Next(4); // choose randomly one direction
            int dx = nX[direction];
            int dy = nY[direction];

            int p1 = firstParticle[c];
            int i = p1;
           

            do // first we empty the perevious positions
            {
                site[x[i], y[i]] = 0;
                x[i] = PBC(x[i] + dx);
                y[i] = PBC(y[i] + dy);
                i = nextParticle[i];
            } while (i != p1);

            do // then fill sites with new cluster label
            {
                site[x[i], y[i]] = c;
                i = nextParticle[i];
            } while (i != p1);


            if(nClusters > 1)
                do // checking if there is neigbour near each particle in cluster c
                {
                    Neighbour(x[i], y[i]);
                    i = nextParticle[i];
                } while (i != p1);

            
        }
    }
}
