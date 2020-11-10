using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class CSVParser : MonoBehaviour {

    const char fieldSeparator = ';';
    const char textMarker = '"';
    const char newLine = (char)10;
    const char nullChar = (char)0;

    [SerializeField] string filePath = default;
    [SerializeField] FilePathType pathType = default;
    [SerializeField] Encoding fileEncoding = Encoding.UTF8;

    public enum FilePathType {
        Absolute,
        StreamingAssets
    }

    void Start () {
        string actualPath = GetPath();
        if(TryRead(actualPath, fileEncoding, out var s)){
            // Debug.Log(s);

            // string chars = string.Empty;
            // foreach(var c in s){
            //     chars += $"{(int)c}\n";
            // }
            // Debug.Log(chars);

            // var lines = string.Empty;
            // var sr = new StringReader(s);
            // string line = sr.ReadLine();
            // while(line != null){
            //     lines += $">{line}<\n";
            //     line = sr.ReadLine();
            // }
            // sr.Close();
            // Debug.Log(lines);
            var result = Parse(s);
            foreach(var row in result){
                var output = string.Empty;
                foreach(var col in row){
                    output += $"{col}\n-----------------------------\n";
                }
                Debug.Log(output);
            }
        }
    }

    string GetPath () {
        switch(pathType){
            case FilePathType.Absolute:
                return filePath;
            case FilePathType.StreamingAssets:
                return $"{Application.streamingAssetsPath}\\{filePath}";
            default:
                Debug.LogError($"Unknown {nameof(FilePathType)} \"{pathType}\"!");
                return string.Empty;
        }
    }

    public static bool TryParse (string filePath ,Encoding fileEncoding, out string[][] rows) {
        if(TryRead(filePath, fileEncoding, out var text)){
            rows = Parse(text);
            return true;
        }
        rows = null;
        return false;
    }

    private static bool TryRead (string filePath, Encoding fileEncoding, out string output) {
        output = string.Empty;
        try{
            string raw = File.ReadAllText(filePath, fileEncoding);
            StringReader sr = new StringReader(raw);
            StringBuilder sb = new StringBuilder();
            string line = sr.ReadLine();
            while(line != null){
                sb.Append($"{line}{newLine}");
                line = sr.ReadLine();
            }
            output = sb.ToString();
            sr.Close();
            return true;
        }catch(System.Exception e){
            Debug.LogError(e);
            return false;
        }
    }

    private static string[][] Parse (string input) {
        List<List<string>> rows = new List<List<string>>();
        List<string> currentRow = new List<string>();
        StringBuilder sb = new StringBuilder();
        bool inText = false;
        char lastChar = nullChar;
        for(int i=0; i<input.Length; i++){
            char c = input[i];
            switch(c){
                case fieldSeparator:
                    HandleEndChar(c);
                    break;
                case newLine:
                    HandleEndChar(c);
                    break;
                case textMarker:
                    HandleTextMarker(ref c, ref i);
                    break;
                default:
                    sb.Append(c);
                    break;
            }
            lastChar = c;
        }
        var output = new string[rows.Count][];
        for(int i=0; i<output.Length; i++){
            output[i] = rows[i].ToArray();
        }
        return output;

        void HandleEndChar (char c){
            if(inText){
                sb.Append(c);
            }else{
                currentRow.Add(sb.ToString());
                sb.Clear();
                if(c == newLine){
                    rows.Add(currentRow);
                    currentRow = new List<string>();
                }
            }
        }

        void HandleTextMarker (ref char c, ref int i){
            try{
                var nextChar = input[i+1];
                if(nextChar == c){
                    sb.Append(c);
                    i++;
                }else{
                    inText = !inText;
                }
            }catch{ }
        }

    }
	
}
