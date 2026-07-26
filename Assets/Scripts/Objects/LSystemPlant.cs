using System.Collections.Generic;
using UnityEngine;

// Draws a fractal curve using turtle graphics.
//
// This is my C# version of the FractalGrammars.java program we were given in class
// (the turtleGraphics method inside CvFractalGrammars), which is explained in
// Ammeraal & Zhang section 8.2. I kept the same idea: the method is recursive, so
// when it meets an F and we are not at the bottom yet, it calls itself on the
// F-string with one less level and a shorter step. It never builds the big expanded
// string, which is nice because that string gets huge very fast.
//
// Two things I had to change to make it work in Unity:
//  1. The Java version draws straight to the screen with Graphics.drawLine(). Unity
//     doesn't work that way, so instead I collect every segment and build them into
//     a mesh at the end, which means the curve is real 3D geometry in the scene.
//  2. The Java version only remembers one saved position (xMark/yMark/dirMark).
//     Section 8.2 says a stack is the right structure for [ and ], so I used a Stack.
//     It also means brackets inside brackets work properly.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LSystemPlant : MonoBehaviour
{
    public enum Preset { Tree1, Dragon, Koch, Hilbert, Sierpinski }
    public Preset preset = Preset.Tree1;

    [Tooltip("How many generations deep to go. The class program calls this the level.")]
    [Range(0, 14)] public int iterations = 4;

    [Tooltip("Length of the first step. Each level down gets shorter than this.")]
    public float initialLength = 6f;

    [Tooltip("Which way the turtle faces at the start. 90 means straight up, same as tree1.txt.")]
    public float startDirection = 90f;

    [Tooltip("Stops Unity freezing if someone sets the level really high.")]
    public int maxSegments = 200000;

    FractalGrammar grammar;

    // Where the turtle is and which way it faces. Same as xLast/yLast/dir in the Java.
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

    // Builds the curve. Right-click the component title in the Inspector to run this
    // without pressing Play, which is handy for checking a grammar looks right.
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

    // The recursive part. This follows the same structure as the Java version.
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

                case 'f':   // step forward without drawing
                    if (depth == 0) posLast = Step(len);
                    else TurtleGraphics(grammar.strf, depth - 1, grammar.reductFact * len);
                    break;

                case 'X':   // X and Y only expand the rules, nothing gets drawn
                    if (depth > 0) TurtleGraphics(grammar.strX, depth - 1, grammar.reductFact * len);
                    break;

                case 'Y':
                    if (depth > 0) TurtleGraphics(grammar.strY, depth - 1, grammar.reductFact * len);
                    break;

                case 'U':   // U and V are the extras the class version added
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

    // Works out where the turtle ends up after stepping forward. Same trig as the
    // Java (dx = len*cos, dy = len*sin), just put on the XY plane so the tree stands up.
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
