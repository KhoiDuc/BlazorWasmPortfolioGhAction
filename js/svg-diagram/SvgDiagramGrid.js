class SvgDiagramGrid {
    static className = "grid";
    static gridLineClassName = "svg-diagram-grid-line";

    #svg;
    #gridGroup;
    #step;
    #shouldShow = false;

    constructor(svg, step = 10) {
        this.#svg = svg;
        this.#step = step;
    }

    get isShown() {
        return !!this.#svg.find(`.${SvgDiagramGrid.className}`).length;
    }

    get shouldShow() {
        return this.#shouldShow;
    }

    set shouldShow(value) {
        this.#shouldShow = value;

        let isShown = this.isShown;

        if (this.#shouldShow) {
            if (!isShown) {
                this.#create();
            }
        } else {
            if (isShown) {
                this.#remove();
            }
        }
    }

    get step() {
        return this.#step;
    }

    set step(value) {
        this.#step = value;
        this.#recreate();
    }

    #recreate() {
        if (this.isShown) {
            this.#remove();
        }

        if (this.#shouldShow) {
            this.#create();
        }
    }

    #create() {
        this.#gridGroup = this.#svg.group().addClass(SvgDiagramGrid.className).back();
        this.#draw();
    }

    #remove() {
        this.#gridGroup.remove();
    }

    #draw() {
        if (!this.#gridGroup) {
            return;
        }

        let width = this.#svg.width();
        let height = this.#svg.height();

        for (let verticalLineX = 0; verticalLineX < width; verticalLineX += this.#step) {
            this.#gridGroup.line(verticalLineX, 0, verticalLineX, height)
                .addClass(SvgDiagramGrid.gridLineClassName);
        }

        for (let horizontalLineY = 0; horizontalLineY < height; horizontalLineY += this.#step) {
            this.#gridGroup.line(0, horizontalLineY, width, horizontalLineY)
                .addClass(SvgDiagramGrid.gridLineClassName);
        }
    }
}