namespace SharpLeftRight
{
    public interface IReadIndicator
    {
        void Arrive();
        void Depart();
        bool IsOccupied { get; }
    }
}