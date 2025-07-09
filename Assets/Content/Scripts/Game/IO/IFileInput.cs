namespace Content.Scripts.Game.IO
{
    public interface IFileInput<T>
    {
        public T LoadData(string path);
    }
}