namespace Framework.InputManagement
{
    public interface ISource<T>
    {
        SourceInfo SourceInfo { get; }
        T GetValue();
    }
}
