//if (!window.ntauth) {
//    window.ntauth = {};
//}

//if (!window.ntauth.grid) {
//    window.ntauth.grid = {};
//}

window.ntech = {
    grid: {
        createGrid: (element) => {
            element.querySelectorAll("th.resizable").forEach(th => {
                const resizer = th.querySelector(".resizer");

                resizer.addEventListener("mousedown", (e) => {
                    const startX = e.pageX;
                    const startWidth = th.offsetWidth;

                    // Graps all other th element in the table, for use later.
                    const table = th.closest("table");
                    const otherThs = Array.from(table.querySelectorAll("th")).filter(otherTh => otherTh !== th);

                    const handleMouseMove = (e) => {

                        // Stores the width of all other th before resizing the current th.
                        const widths = new Map();
                        otherThs.forEach(otherTh => {
                            widths.set(otherTh, otherTh.offsetWidth);
                        });

                        // Resizing the current th.
                        const newWidth = startWidth + (e.pageX - startX);
                        th.style.width = `${newWidth}px`;

                        // Restoring the width of all other th after resizing the current th.
                        widths.forEach((width, otherTh) => {
                            otherTh.style.width = `${width}px`;
                        });
                    };

                    const handleMouseUp = () => {
                        document.removeEventListener("mousemove", handleMouseMove);
                        document.removeEventListener("mouseup", handleMouseUp);
                    };

                    document.addEventListener("mousemove", handleMouseMove);
                    document.addEventListener("mouseup", handleMouseUp);
                });
            });
        }
    },
    contextMenu: {
        create: (element) => {
            document.addEventListener('contextmenu', (e) => {
                //alert("You've tried to open context menu");
                e.preventDefault();

                console.log(e);

                element.style.display = "block";
                element.style.top = e.pageY + "px",
                element.style.left = e.pageX + "px"

            }, false);
        }
    }
}

window.ntauth = {
    grid: {
        grids: { },
        create: (id, columns = [], rows = [], data = []) => {
            console.log(columns);
            console.log(rows);

            var content = [];
            for (i = 0; i < rows.length; i++) {
                console.log(rows[i]);
                var row = [];
                for (j = 0; j < data.length; j++) {
                    console.log(rows[i][data[j]]);
                    console.log(data[j]);
                    row.push(rows[i][data[j]]);
                }
                content.push(row);
            }
            console.log(content);

            window.ntauth.grid.grids[id] = new gridjs.Grid({
                columns: columns,
                data: content,
                resizable: true,
                sort: true
            }).render(document.getElementById(id));
        }
    }
}
