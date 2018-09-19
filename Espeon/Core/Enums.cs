namespace Espeon.Core
{
    public enum LogSource
    {
        Database,
        CustomCmds,
        Pokemon
    }

    public enum SpecialRole
    {
        Admin,
        Mod
    }

    public enum Face
    {
        Heads,
        Tails
    }

    //future proofing
    public enum ServiceType
    {
        Singleton,
        Transient,
        Scoped
    }
}
