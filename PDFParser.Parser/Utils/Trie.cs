using System.Diagnostics.CodeAnalysis;

namespace PDFParser.Parser.Utils;

public interface ITrie
{
    public void Insert(string value);
    public bool Contains(string value);
    public void Clear();
    public List<string> Search(string prefix);
}

public class Trie : ITrie
{
    private sealed class Node
    {
        public readonly Dictionary<char, Node> SubNodes = new();
        public string? Word { get; set; }

        public bool IsWord => Word != null;
    }

    private Node _root = new();
    
    public Trie() {}
    
    public void Insert(string value)
    {
        var current = _root;
        foreach (var c in value)
        {
            if (current.SubNodes.TryGetValue(c, out var node))
            {
                current = node;
            }
            else
            {
                current.SubNodes[c] = new Node();
                current = current.SubNodes[c];
            }
        }
        
        current.Word = value;
    }

    public bool Contains(string value)
    {
        var current = _root;
        foreach (var c in value)
        {
            if (current.SubNodes.TryGetValue(c, out var node))
            {
                current = node;
            }
        }
        
        return current.IsWord && current.Word == value;
    }

    public void Clear()
    {
        _root = new Node();
    }

    public List<string> Search(string prefix)
    {
        var current = _root;
        foreach (var c in prefix)
        {
            if (current.SubNodes.ContainsKey(c))
            {
                current = current.SubNodes[c];
            }
            else
            {
                break;
            }
        }

        //Tree traversal
        var stack = new Stack<Node>();
        stack.Push(current);
        var results = new List<string>();
        
        while (stack.Any())
        {
            var currNode = stack.Pop();
            if (currNode.IsWord)
            {
                //TODO: figure out how to get the word.
                results.Add(currNode.Word!);
            }
            
            foreach (var (c, subNode) in currNode.SubNodes)
            {
                stack.Push(subNode);
            }
        }

        return results;
    }
}

public class CompressedTrie : ITrie
{
    private sealed class Node
    {
        //here string represents a prefix
        public Dictionary<char, Node> SubNodes = new();
        public string? Word { get; set; }

        public bool IsTerminal => Word != null;

        public string Label { get; set; } = string.Empty;
        
        public Node()
        {
        }

        public Node(string value)
        {
            Label = value;
        }
    }
    
    private Node _root = new();
    
    public CompressedTrie()
    {
    }

    //Longest Common Prefix
    private string ComputeLCP(string a, string b)
    {
        var index = 0;
        var minLen = Math.Min(a.Length, b.Length);
        while (index < minLen && a[index] == b[index])
        {
            index++;
        }

        return a[..index];
    }
    
    public void Insert(string value)
    {
        var current = _root;
        var suffix = value;
        while (suffix.Length > 0)
        {
            if (current.SubNodes.TryGetValue(suffix[0], out var node))
            {
                var lcp = ComputeLCP(node.Label, suffix);
                if (lcp == node.Label)
                {
                    //Case 1. Extend the tree from LCP node.
                    current = node;
                    suffix = suffix[lcp.Length..];
                }
                else
                {
                    //Delete old edge. Create split node edge.
                    current.SubNodes.Remove(node.Label[0]);
                    current.SubNodes.Add(lcp[0], new Node(lcp));
                    current = current.SubNodes[lcp[0]];
                    
                    //Ok, now, create two paths
                    //1. existing node
                    var newSuffix = node.Label[lcp.Length..];
                    node.Label = newSuffix;
                    current.SubNodes[newSuffix[0]] = node;
                    
                    //2. value node
                    suffix = suffix[lcp.Length..];
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (suffix.Length >= 1)
        {
            current.SubNodes[suffix[0]] = new Node(suffix)
                { Word = value };   
        }
        else
        {
            current.Word = value;
        }
    }

    public bool Contains(string value)
    {
        var current = _root;
        var suffix = value;

        while (suffix.Length > 0)
        {
            if (current.SubNodes.TryGetValue(suffix[0], out var node))
            {
                current = node;

                if (node.Label.Length > suffix.Length)
                {
                    return false;
                }
                
                suffix = suffix[node.Label.Length..];
                continue;
            }
            break;
        }
        
        return current.Word == value;
    }

    public void Clear()
    {
        _root = new Node();
    }

    public List<string> Search(string prefix)
    {
        var current = _root;
        var suffix = prefix;
        
        while (suffix.Length > 0)
        {
            if (current.SubNodes.TryGetValue(suffix[0], out var node))
            {
                current = node;

                if (current.Label.Length > suffix.Length)
                {
                    if (node.Label.StartsWith(suffix))
                    {
                        break;
                    }
                    else
                    {
                        return [];
                    }
                }
                
                suffix = suffix[node.Label.Length..];
            }
            else
            {
                return [];
            }
        }
        
        //Tree traversal... I guess?
        var results = new List<string>();
        var stack = new Stack<Node>();
        stack.Push(current);

        while (stack.Any())
        {
            var node = stack.Pop();
            if (node.Word != null)
            {
                results.Add(node.Word);
            }

            foreach (var (key, subNode) in node.SubNodes)
            {
                stack.Push(subNode);
            }
            
        }
        
        return results;
    }
}