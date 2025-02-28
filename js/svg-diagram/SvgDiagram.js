class SvgDiagram {
    #svgId;
    #grid;

    #selectionControls;
    #movingControls;

    svg;
    shapeGenerator;

    constructor(svgId, gridStep) {
        this.#svgId = svgId;
        this.svg = SVG(`#${this.#svgId}`);
        if (!this.#svgId) {
            return;
        }

        this.#grid = new SvgDiagramGrid(this.svg, gridStep);

        this.#selectionControls = new SvgDiagramSelectionControls(this.svg);
        this.#movingControls = new SvgDiagramMovingControls(this.svg, this.#selectionControls);
        this.shapeGenerator = new ShapeGenerator(this.svg, this.#selectionControls);
    }

    setParameters(width, height, shouldShowGrid, gridSetp) {
        if (!this.#svgId) {
            return;
        }

        this.svg.size(width, height);
        this.#grid.step = gridSetp;
        this.#grid.shouldShow = shouldShowGrid;
    }

    removeSelectedElements() {
        this.#selectionControls.removeSelectedElements();
    }
}