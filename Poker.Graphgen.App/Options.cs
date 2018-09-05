using CommandLine;

namespace Poker.Graphgen.App
{
    public class Options
    {
        [Option('c', "config", Required = false, HelpText = "Path to json file with config. If not provided, default configuration is used (use [-g] option to generate it to json file).")]
        public string ConfigFileName { get; set; }

        [Option('x', "xml", Required = false, HelpText = "If set, only xml is generated.")]
        public string XmlOnlyFile { get; set; }

        [Option('i', "iter", Required = false, HelpText = "Number of training iterations. The default value is 5000.", Default = 5000)]
        public int Iterations { get; set; }

        [Option('t', "tblName", Required = false, HelpText = "Name of output table name. Default is nodes1.", Default = "nodes1")]
        public string TableName { get; set; }

        [Option('g', "genConf", Required = false, HelpText = "Generate example config file to default.json and exit the program. It can be used to build your own configuration.")]
        public bool GenConfig { get; set; }

        [Option('s', "silent", Required = false, HelpText = "No messages written on console")]
        public bool Silent { get; set; }
    }
}