module FileConventions.Test.WrapTextTests

open System

open NUnit.Framework

open FileConventions

[<Test>]
let WrapTextTest1() =
    let characterCount = 64

    let paragraph =
        "This is a very very very very long line with more than 64 characters."

    let expectedResult =
        "This is a very very very very long line with more than 64"
        + Environment.NewLine
        + "characters."

    Assert.That(WrapText paragraph characterCount, Is.EqualTo expectedResult)

[<Test>]
let WrapTextTest2() =
    let characterCount = 64

    let paragraph =
        "This is short line."
        + Environment.NewLine
        + "```"
        + Environment.NewLine
        + "This is a very very very very long line with more than 64 characters inside a code block."
        + Environment.NewLine
        + "```"

    let expectedResult = paragraph

    Assert.That(WrapText paragraph characterCount, Is.EqualTo expectedResult)

[<Test>]
let WrapTextTest3() =
    let characterCount = 64
    let tenDigits = "1234567890"

    let seventyChars =
        tenDigits
        + tenDigits
        + tenDigits
        + tenDigits
        + tenDigits
        + tenDigits
        + tenDigits

    let paragraph =
        "This is short line referring to [1]."
        + Environment.NewLine
        + "[1] someUrl://"
        + seventyChars

    let expectedResult = paragraph

    Assert.That(WrapText paragraph characterCount, Is.EqualTo expectedResult)


[<Test>]
let WrapTextTest4() =
    let characterCount = 64

    let text =
        "This is short line."
        + Environment.NewLine
        + Environment.NewLine
        + "This is a very very very very very long line with more than 64 characters."

    let expectedResult =
        "This is short line."
        + Environment.NewLine
        + Environment.NewLine
        + "This is a very very very very very long line with more than 64"
        + Environment.NewLine
        + "characters."

    Assert.That(WrapText text characterCount, Is.EqualTo expectedResult)
