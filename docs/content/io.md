+++
title          = "IO"
show_nav       = true
nav_back_label = "Http"
nav_back_url   = "/dotnet-util/http"
nav_next_label = "Math"
nav_next_url   = "/dotnet-util/math"
+++

## Features

- `FileReader` (synchronous): read a file as a string, as an array of lines, as a separator-delimited dictionary, or deserialize its JSON content into a typed object.
- `FileReaderAsync` (asynchronous): the same four operations as `FileReader`, returning `Task`-wrapped results.

## Class Diagram

```mermaid
classDiagram
    namespace IO {
        class FileReader {
            <<static>>
            +string Read(string path)
            +string[] ReadLines(string path)
            +Dictionary~string, string[]~ ReadAsDictionary(string path, char separator)
            +T ReadAndDeserialize~T~(string path)
        }
        class FileReaderAsync {
            <<static>>
            +Task~string~ ReadAsync(string path)
            +Task~string[]~ ReadLinesAsync(string path)
            +Task~Dictionary~string, string[]~~ ReadAsDictionaryAsync(string path, char separator)
            +Task~T~ ReadAndDeserializeAsync~T~(string path)
        }
    }
```

## Usage

### Synchronous

```csharp
using ArturRios.Util.IO;

// Entire file as a single string
string content = FileReader.Read("/data/notes.txt");

// Array of lines
string[] lines = FileReader.ReadLines("/data/notes.txt");

// Separator-delimited file → dictionary
// Key = first column value, Value = remaining columns as string[]
Dictionary<string, string[]> table = FileReader.ReadAsDictionary("/data/config.csv", ',');

// JSON file → typed object (uses Newtonsoft.Json)
AppConfig config = FileReader.ReadAndDeserialize<AppConfig>("/data/config.json");
```

### Asynchronous

```csharp
using ArturRios.Util.IO;

string content = await FileReaderAsync.ReadAsync("/data/notes.txt");

string[] lines = await FileReaderAsync.ReadLinesAsync("/data/notes.txt");

Dictionary<string, string[]> table =
    await FileReaderAsync.ReadAsDictionaryAsync("/data/config.csv", ',');

AppConfig config =
    await FileReaderAsync.ReadAndDeserializeAsync<AppConfig>("/data/config.json");
```
