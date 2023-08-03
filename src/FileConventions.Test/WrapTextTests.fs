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

[<Test>]
let WrapTextTest5() =
    let characterCount = 64

    let text =
        "Fixed bug (a title of less than 50 chars)"
        + Environment.NewLine
        + Environment.NewLine
        + "These were the steps to reproduce:"
        + Environment.NewLine
        + "Do foo."
        + Environment.NewLine
        + Environment.NewLine
        + "Current results:"
        + Environment.NewLine
        + "Bar happens."
        + Environment.NewLine
        + Environment.NewLine
        + "Expected results:"
        + Environment.NewLine
        + "Baz happens."

    let expectedResult = text

    Assert.That(WrapText text characterCount, Is.EqualTo expectedResult)

[<Test>]
let WrapTextTest6() =
    let characterCount = 64

    let commitMsg =
        "foo: this is a header"
        + Environment.NewLine
        + Environment.NewLine
        + "This is a body:"
        + Environment.NewLine
        + Environment.NewLine
        + "```"
        + Environment.NewLine
        + "A code block that has two conditions, it has a very long line that exceeds the limit."
        + Environment.NewLine
        + Environment.NewLine
        + "It also has multiple paragraphs."
        + Environment.NewLine
        + "```"

    Assert.That(WrapText commitMsg characterCount, Is.EqualTo commitMsg)
