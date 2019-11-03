
namespace Framework.AssetBundles
{
    /// <summary>
    /// Describes a directory which stores asset bundles.
    /// </summary>
    internal struct BundleDirectory
    {
        /// <summary>
        /// The absolute path to the bundle directory.
        /// </summary>
        public readonly string path;
        /// <summary>
        /// The priority of this directory.
        /// </summary>
        public readonly int priority;

        /// <summary>
        /// Creates a new bundle directory description.
        /// </summary>
        /// <param name="path">The directory to add.</param>
        /// <param name="priority">If asset bundles of the same name are found under multiple directories,
        /// the bundle from the directory with the highest priority will be loaded.</param>
        /// <returns>True if a new bundle directory was successfully added.</returns>
        public BundleDirectory(string path, int priority)
        {
            this.path = path;
            this.priority = priority;
        }

        public override string ToString() => path;
    }
}
