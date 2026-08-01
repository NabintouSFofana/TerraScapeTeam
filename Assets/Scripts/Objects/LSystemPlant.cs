using System.Collections.Generic;
using UnityEngine;

// Draws a fractal curve from a string grammar using turtle graphics.
// Owner: Nabintou (object placement).

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LSystemPlant : MonoBehaviour
{
    public enum Preset { Tree1, Dragon, Koch, Hilbert, Sierpinski }
    public Preset preset = Preset.Tree1;

    [Tooltip("Generation / recursion level (the original program's 'level').")]
    [Range(0, 14)] public int iterations = 4;

    [Tooltip("Length of the very first step, before reductFact shrinks it.")]
    public float initialLength = 6f;

    [Tooltip("Starting direction in degrees. 90 = pointing up (as in tree1.txt).")]
    public float startDirection = 90f;

    [Tooltip("Safety limit so a high generation cannot freeze the editor.")]
    public int maxSegments = 200000;

    FractalGrammar grammar;

    // Turtle state
    Vector3 posLast;
    float dir;

    readonly List<Vector3> verts = new List<Vector3>();
    readonly List<int> indices = new List<int>();
    readonly Stack<TurtleState> stack = new Stack<TurtleState>();

    struct TurtleState
    {
        public Vector3 position;
        public float direction;
    }

    void Start()
    {
        Build();
    }

    // Rebuild the curve
    [ContextMenu("Rebuild")]
    public void Build()
    {
        grammar = GrammarFor(preset);

        verts.Clear();
        indices.Clear();
        stack.Clear();
        posLast = Vector3.zero;
        dir = startDirection;

        TurtleGraphics(grammar.axiom, iterations, initialLength);
        BuildMesh();
    }

    static FractalGrammar GrammarFor(Preset p)
    {
        switch (p)
        {
            case Preset.Dragon:     return FractalGrammar.Dragon();
            case Preset.Koch:       return FractalGrammar.Koch();
            case Preset.Hilbert:    return FractalGrammar.Hilbert();
            case Preset.Sierpinski: return FractalGrammar.Sierpinski();
            default:                return FractalGrammar.Tree1();
        }
    }

    // Recursive turtle interpreter
    void TurtleGraphics(string instruction, int depth, float len)
    {
        if (string.IsNullOrEmpty(instruction)) return;
        if (verts.Count >= maxSegments * 2) return;

        for (int i = 0; i < instruction.Length; i++)
        {
            switch (instruction[i])
            {
                case 'F':   // step forward and draw
                    if (depth == 0) DrawTo(Step(len));
                    else TurtleGraphics(grammar.strF, depth - 1, grammar.reductFact * len);
                    break;

                case 'f':   
                    if (depth == 0) posLast = Step(len);
                    else TurtleGraphics(grammar.strf, depth - 1, grammar.reductFact * len);
                    break;

                case 'X':   
                    if (depth > 0) TurtleGraphics(grammar.strX, depth - 1, grammar.reductFact * len);
                    break;

                case 'Y':
                    if (depth > 0) TurtleGraphics(grammar.strY, depth - 1, grammar.reductFact * len);
                    break;

                case 'U':   
                    if (depth > 0) TurtleGraphics(grammar.strU, depth - 1, grammar.reductFact * len);
                    break;

                case 'V':
                    if (depth > 0) TurtleGraphics(grammar.strV, depth - 1, grammar.reductFact * len);
                    break;

                case '+':   // turn right (original: dir -= rotation)
                    dir -= grammar.rotation;
                    break;

                case '-':   // turn left (original: dir += rotation)
                    dir += grammar.rotation;
                    break;

                case '[':   // store the current state
                    stack.Push(new TurtleState { position = posLast, direction = dir });
                    break;

                case ']':   // restore the previously stored state
                    if (stack.Count > 0)
                    {
                        TurtleState s = stack.Pop();
                        posLast = s.position;
                        dir = s.direction;
                    }
                    break;
            }
        }
    }

    Vector3 Step(float len)
    {
        float rad = Mathf.Deg2Rad * dir;
        return posLast + new Vector3(len * Mathf.Cos(rad), len * Mathf.Sin(rad), 0f);
    }

    void DrawTo(Vector3 p)
    {
        verts.Add(posLast);
        verts.Add(p);
        indices.Add(verts.Count - 2);
        indices.Add(verts.Count - 1);
        posLast = p;
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh { name = "Fractal_" + preset };
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;

        var mr = GetComponent<MeshRenderer>();
        if (mr.sharedMaterial == null)
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader != null)
            {
                var m = new Material(shader);
                m.color = (preset == Preset.Tree1)
                    ? new Color(0.36f, 0.25f, 0.12f)   // brown
                    : new Color(0.10f, 0.60f, 0.90f);  // blue
                mr.sharedMaterial = m;
            }
        }
    }
}
