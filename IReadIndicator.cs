namespace SharpLeftRight
{
    interface IReadIndicator
    {
        void Arrive();
        void Depart();
        bool IsOccupied { get };
    }
}