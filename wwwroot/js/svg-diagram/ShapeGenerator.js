class ShapeGenerator {
    static colorLetters = '0123456789ABCDEF';
    static colorPrefix = '#';
    static strokeWidth = 5;

    #svg;
    #selectionControls;

    constructor(svg, selectionControls) {
        this.#svg = svg;
        this.#selectionControls = selectionControls;
    }

    addRectangle() {
        var maxWidth = Math.ceil(this.#svg.width() / 2.0);
        var maxHeight = Math.ceil(this.#svg.height() / 2.0);

        const desirableMinSide = 10;

        var minWidth = Math.min(desirableMinSide, maxWidth);
        var minHeight = Math.min(desirableMinSide, maxHeight);

        var rectangle = this.#svg
            .rect(ShapeGenerator.getRandomInt(minWidth, maxWidth),
                ShapeGenerator.getRandomInt(minHeight, maxHeight))
            .move(ShapeGenerator.getRandomInt(0, maxWidth),
                ShapeGenerator.getRandomInt(0, maxHeight));

        ShapeGenerator.setStroke(rectangle);
        ShapeGenerator.setFill(rectangle);

        this.#selectionControls.addSelectableElement(rectangle);

        return rectangle;
    }

    addCircle() {
        var maxWidth = Math.ceil(this.#svg.width() / 2.0);
        var maxHeight = Math.ceil(this.#svg.height() / 2.0);
        var maxDiameter = Math.min(maxWidth, maxHeight);

        const desirableMinDiameter = 10;

        var minDiameter = Math.min(desirableMinDiameter, maxDiameter);

        var circle = this.#svg
            .circle(ShapeGenerator.getRandomInt(minDiameter, maxDiameter))
            .move(ShapeGenerator.getRandomInt(0, maxWidth),
                ShapeGenerator.getRandomInt(0, maxHeight));

        ShapeGenerator.setStroke(circle);
        ShapeGenerator.setFill(circle);

        this.#selectionControls.addSelectableElement(circle);

        return circle;
    }

    addLine() {
        var maxWidth = Math.ceil(this.#svg.width() / 2.0);
        var maxHeight = Math.ceil(this.#svg.height() / 2.0);

        var line = this.#svg
            .line(ShapeGenerator.getRandomInt(0, maxWidth),
                ShapeGenerator.getRandomInt(0, maxHeight),
                ShapeGenerator.getRandomInt(0, maxWidth),
                ShapeGenerator.getRandomInt(0, maxHeight))
            .move(ShapeGenerator.getRandomInt(0, maxWidth),
                ShapeGenerator.getRandomInt(0, maxHeight));

        ShapeGenerator.setStroke(line);

        this.#selectionControls.addSelectableElement(line);

        return line;
    }

    static setStroke(element) {
        element.stroke({
            color: ShapeGenerator.getRandomColor(),
            width: ShapeGenerator.strokeWidth
        });
    }

    static setFill(element) {
        element.fill(ShapeGenerator.getRandomColor());
    }

    static getRandomColor() {
        var color = ShapeGenerator.colorPrefix;
        for (var colorPositionIndex = 0; colorPositionIndex < 6; colorPositionIndex++) {
            color += ShapeGenerator.colorLetters[Math.floor(Math.random() * 16)];
        }
        return color;
    }

    static getRandomInt(min, max) {
        min = Math.ceil(min);
        max = Math.floor(max);
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }
}