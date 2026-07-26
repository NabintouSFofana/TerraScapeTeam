// Holds the rules that describe one fractal curve.
//
// Our textbook (Ammeraal & Zhang, "Computer Graphics for Java Programmers" 3rd ed.,
// section 8.2 "String Grammars") writes a grammar as six things:
//     (axiom, F-string, f-string, X-string, Y-string, angle)
// The FractalGrammars.java example we were given in class adds U- and V-strings on
// top of that, so I kept those as well. Where the book says "nil" I just use an
// empty string.
//
// What each character means when the turtle reads it (section 8.2):
//   F  move forward and draw a line
//   f  move forward without drawing
//   +  turn right by the angle
//   -  turn left by the angle
//   [  remember where we are
//   ]  go back to where we were
//   X, Y, U, V  these only matter when expanding the rules, the turtle skips them
//
// The grammars at the bottom are the ones listed in section 8.2. Tree1 is the same
// one that came in the tree1.txt data file with the class example.
public class FractalGrammar
{
    public string axiom = "F";
    public string strF = "";
    public string strf = "";
    public string strX = "";
    public string strY = "";
    public string strU = "";
    public string strV = "";

    // How far the turtle turns on a + or -, in degrees.
    public float rotation = 25.7f;

    // Every time we go one level deeper the step length gets multiplied by this,
    // so the small branches come out smaller than the trunk. The class program
    // calls this the reduction factor.
    public float reductFact = 0.34f;

    // ---- The grammars listed in section 8.2 ----

    // Tree1: (F, F[+F]F[-F]F, nil, nil, nil, 25.7)
    // Same one as the tree1.txt file that came with the class example.
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
