






using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif


[assembly: AssemblyTitle("Monkeyspeak")]
[assembly: AssemblyDescription("Scripting language that is very user friendly.")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Monkeyspeak")]
[assembly: AssemblyCopyright("Copyright ©  2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("36caff91-3743-468a-bdde-a2305feefccf")]

//[assembly: AssemblyVersion("1.0.03.6816")]
//[assembly: AssemblyFileVersion("1.0.03.6816")]

[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("1.0")]

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("Monkeyspeak.snk")]
[assembly: AssemblyKeyName("")]
