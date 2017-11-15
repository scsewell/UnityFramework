namespace Framework.Interpolation
{
    public interface IInterpolator
    {
        void FixedFrame();
        void UpdateFrame(float fac);
    }
}