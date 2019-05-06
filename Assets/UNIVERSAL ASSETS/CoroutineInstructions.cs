public class CoroutineInstructions {

    public readonly float delay;
    public readonly bool invertEvalInput;
    public readonly bool invertEvalOutput;

    private CoroutineInstructions (float delay = 0f, bool invertEvalInput = false, bool invertEvalOutput = false) {
        this.delay = delay;
        this.invertEvalInput = invertEvalInput;
        this.invertEvalOutput = invertEvalOutput;
    }

    public static CoroutineInstructions Combine (params CoroutineInstructions[] instructions) {
        float delay = 0;
        bool iEvalIn = false;
        bool iEvalOut = false;
        foreach(var instruction in instructions){
            delay += instruction.delay;
            iEvalIn = iEvalIn ^ instruction.invertEvalInput;
            iEvalOut = iEvalOut ^ instruction.invertEvalOutput;
        }
        return new CoroutineInstructions(
            delay: delay,
            invertEvalInput: iEvalIn,
            invertEvalOutput: iEvalOut
        );
    }

    public static CoroutineInstructions Delay (float delay) {
        return new CoroutineInstructions (delay: delay);
    }

    public static CoroutineInstructions InvertEvalInput () {
        return new CoroutineInstructions (invertEvalInput: true);
    }

    public static CoroutineInstructions InvertEvalOutput () {
        return new CoroutineInstructions (invertEvalOutput: true);
    }
    
}
