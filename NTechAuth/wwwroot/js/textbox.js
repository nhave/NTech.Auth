export function FormatLineNumbers(id) {
    const elem = document.getElementById(id);
    const totalLines = elem.childElementCount;

    const digits = totalLines.toString().length;
    console.log(digits);
    const lineNumberWidth = 22.7 + ((digits - 1) * 7.7);
    console.log(lineNumberWidth);

    const children = elem.childNodes;
    children.forEach(
        function (node, index) {
            if (node.nodeName == "SPAN") {
                node.style.setProperty('--nt-line-numbers-width', `${lineNumberWidth}px`);
            }
        }
    );
}