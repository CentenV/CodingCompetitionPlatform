function calculator(num1, symbol, num2) {
    switch (symbol)
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
            console.log("Invalid Symbol");
            return 0;
    }
}