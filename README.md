[![Downloads](https://img.shields.io/nuget/dt/Join.CSharp.svg)](https://www.nuget.org/packages/Join.CSharp)

## Join CSharp

Simple, Roslyn-based tool to join a bunch of .cs files into one.

## Installation

The tool can be installed from nuget as a .Net Core global tool as follows:

    dotnet tool install -g Join.CSharp

## Usage

    joincs:
      A command line tool to merge a set of C# files into one single file.

    Usage:
      joincs [options]

    Options:
      --input <input>                                        The folder containing the C# files you want to merge
      --output <output>                                      Target file name (e.g. 'output.cs'). If not provided, the output is written to the console. [default: ]
      --include-assembly-attributes                          [default: False]
      --preprocessor-directives <preprocessor-directives>    A list of preprocessor directives that should be defined. Code between undefined #if/#endif directives is ignored. [default: ]
      --version                                              Show version information
      -?, -h, --help                                         Show help and usage information

If no output file is specified, the result is written to the console.

