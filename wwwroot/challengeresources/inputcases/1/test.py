def calculator(num1, symbol, num2):
    if symbol == "+":
        return num1 + num2
    elif symbol == "-":
        return num1 - num2
    elif symbol == "*":
        return num1 * num2
    elif symbol == "/":
        return num1 / num2
    else:
        print("Invalid Symbol")


###### INJECTED CODE ######
if __name__ == "__main__":
    print(calculator(100000, "*", 10000))
###### INJECTED CODE ######