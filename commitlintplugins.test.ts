const { spawnSync } = require('child_process');

test('type-space-after-colon1', () => {
    let commitMsgWithNoSpace = 'foo:bar'
    let typeSpaceAfterColon1 = spawnSync('npx', ['commitlint', '--verbose'], { input: commitMsgWithNoSpace });
    expect(typeSpaceAfterColon1.status).not.toBe(0);
});


test('type-space-after-colon2', () => {
    let commitMsgWithSpace = 'foo: bar'
    let typeSpaceAfterColon2 = spawnSync('npx', ['commitlint', '--verbose'], { input: commitMsgWithSpace });
    expect(typeSpaceAfterColon2.status).toBe(0);
});


test('type-space-after-colon3', () => {
    let commitMsgWithNoSpaceBeforeColonButAtTheEnd = 'foo: a tale of bar:baz'
    let typeSpaceAfterColon3 = spawnSync('npx', ['commitlint', '--verbose'], { input: commitMsgWithNoSpaceBeforeColonButAtTheEnd });
    expect(typeSpaceAfterColon3.status).toBe(0);
});


test('subject-lowercase1', () => {
    let commitMsgWithUppercaseAfterColon = "foo: Bar baz"
    let subjectLowerCase1 = spawnSync('npx', ['commitlint', '--verbose'], { input: commitMsgWithUppercaseAfterColon });
    expect(subjectLowerCase1.status).not.toBe(0);
});


test('subject-lowercase2', () => {
    let commitMsgWithLowercaseAfterColon = "foo: bar baz"
    let subjectLowerCase2 = spawnSync('npx', ['commitlint', '--verbose'], { input: commitMsgWithLowercaseAfterColon });
    expect(subjectLowerCase2.status).toBe(0);
});


test('subject-lowercase3', () => {
    let commitMsgWithAcronymAfterColon = "foo: BAR baz"
    let subjectLowerCase3 = spawnSync('npx', ['commitlint', '--verbose'], { input: commitMsgWithAcronymAfterColon });
    expect(subjectLowerCase3.status).toBe(0);
});


test('subject-lowercase4', () => {
    let commitMsgWithNonAlphanumericAfterColon = "foo: 3 tests added"
    let subjectLowerCase4 = spawnSync('npx', ['commitlint', '--verbose'], { input: commitMsgWithNonAlphanumericAfterColon });
    //console.log("=============>" + subjectLowerCase4.stdout);
    expect(subjectLowerCase4.status).toBe(0);
});
