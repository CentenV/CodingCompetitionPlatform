class solution
{
    public static int calculator(int num1, String op, int num2)
    {
        switch(op)
        {
            case "+":
                return num1 + num2;
            case "-":
                return num1 - num2;
            case "*":
                return num1 * num2;
            case "/":
                return num1 / num2;
            default:
                System.out.println("Invalid Symbol");
                return 0;
        }
    }
}