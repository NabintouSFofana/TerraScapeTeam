// Owner: Nabintou (object placement).

public class FractalGrammar
{
    public string axiom = "F";
    public string strF = "";
    public string strf = "";
    public string strX = "";
    public string strY = "";
    public string strU = "";
    public string strV = "";

    // Turning angle alpha, in degrees.
    public float rotation = 25.7f;

    // Length is multiplied by this at every recursion level (the course program's
    // "reduction factor"). Keeps deeper generations smaller.
    public float reductFact = 0.34f;


    // Tree1: (F, F[+F]F[-F]F, nil, nil, nil, 25.7)
    // This is the grammar in the course data file tree1.txt.
    public static FractalGrammar Tree1() => new FractalGrammar
    {
        axiom = "F",
        strF = "F[+F]F[-F]F",
        rotation = 25.7f,
        reductFact = 0.34f
    };

    // Dragon curve: (X, F, nil, X+YF+, FX-Y, 90)
    public static FractalGrammar Dragon() => new FractalGrammar
    {
        axiom = "X",
        strF = "F",
        strX = "X+YF+",
        strY = "FX-Y",
        rotation = 90f,
        reductFact = 0.707f
    };

    // Koch curve: (F, F-F++F-F, nil, nil, nil, 60)
    public static FractalGrammar Koch() => new FractalGrammar
    {
        axiom = "F",
        strF = "F-F++F-F",
        rotation = 60f,
        reductFact = 1f / 3f
    };

    // Hilbert curve: (X, F, nil, YF+XFX+FY, +XF-YFY-FX+, 90)
    public static FractalGrammar Hilbert() => new FractalGrammar
    {
        axiom = "X",
        strF = "F",
        strX = "YF+XFX+FY",
        strY = "+XF-YFY-FX+",
        rotation = 90f,
        reductFact = 0.5f
    };

    // Sierpinski arrowhead: (YF, F, nil, YF+XF+Y, XF-YF-X, 60)
    public static FractalGrammar Sierpinski() => new FractalGrammar
    {
        axiom = "YF",
        strF = "F",
        strX = "YF+XF+Y",
        strY = "XF-YF-X",
        rotation = 60f,
        reductFact = 0.5f
    };
}
