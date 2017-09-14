namespace MikuHatsune10thTSP
{
    public class Pair<T, U>
    {
        T first;
        U second;
        public Pair()
        {

        }
        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get => first; set => first = value; }
        public U Second { get => second; set => second = value; }

        internal object Clone()
        {
            return MemberwiseClone();
        }
    }
}