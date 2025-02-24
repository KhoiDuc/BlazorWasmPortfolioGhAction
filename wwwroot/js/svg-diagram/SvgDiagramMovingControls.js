class SvgDiagramMovingControls {
    static mouseMoveEventName = "mousemove";
    static mouseDownEventName = "mousedown";
    static mouseUpEventName = "mouseup";
    static mouseOutEventName = "mouseout";

    #svg;

    selectionControls;
    mouseX;
    mouseY;

    constructor(svg, selectionControls) {
        this.#svg = svg;
        this.selectionControls = selectionControls;

        let controls = this;

        this.#svg.node.addEventListener(
            SvgDiagramSelectionControls.selectedElementsWillBeChangedEventName,
            (event) => SvgDiagramMovingControls.onSelectedElementsWillBeChanged(controls,
                event.detail.selectedElements));

        this.#svg.node.addEventListener(
            SvgDiagramSelectionControls.selectedElementsChangedEventName,
            (event) => SvgDiagramMovingControls.onSelectedElementsChanged(controls,
                event.detail.selectedElements));
    }

    static onSelectedElementsWillBeChanged(controls, selectedElements) {
        if (!selectedElements.length) {
            return;
        }

        for (let elementIndex = 0; elementIndex < selectedElements.length; elementIndex++) {
            let selectedElement = selectedElements[elementIndex];

            selectedElement.off(SvgDiagramMovingControls.mouseMoveEventName,
                (event) => SvgDiagramMovingControls.onSelectedElementPointerMove
                    (event, controls, selectedElement));

            selectedElement.off(SvgDiagramMovingControls.mouseDownEventName,
                (event) => SvgDiagramMovingControls.onSelectedElementPointerDown
                    (event, controls, selectedElement));

            selectedElement.off(SvgDiagramMovingControls.mouseUpEventName,
                (event) => SvgDiagramMovingControls.onSelectedElementPointerUp
                    (event, controls, selectedElement));
        }
    }

    static onSelectedElementsChanged(controls, selectedElements) {
        if (!selectedElements.length) {
            return;
        }

        for (let elementIndex = 0; elementIndex < selectedElements.length; elementIndex++) {
            let selectedElement = selectedElements[elementIndex];

            selectedElement.on(SvgDiagramMovingControls.mouseMoveEventName,
                (event) => SvgDiagramMovingControls.onSelectedElementPointerMove(event, controls));

            selectedElement.on(SvgDiagramMovingControls.mouseDownEventName,
                (event) => SvgDiagramMovingControls.onSelectedElementPointerDown(event, controls));

            selectedElement.on(SvgDiagramMovingControls.mouseUpEventName,
                (event) => SvgDiagramMovingControls.onSelectedElementPointerUp(controls));
        }
    }

    static onSelectedElementPointerDown(event, controls) {
        controls.mouseX = event.clientX;
        controls.mouseY = event.clientY;
    }

    static onSelectedElementPointerUp(controls) {
        controls.mouseX = null;
        controls.mouseY = null;
    }

    static onSelectedElementPointerMove(event, controls) {
        if (controls.mouseX == null && controls.mouseY == null) {
            return;
        }

        let xOffset = event.clientX - controls.mouseX;
        let yOffset = event.clientY - controls.mouseY;

        controls.mouseX = event.clientX;
        controls.mouseY = event.clientY;

        controls.selectionControls.moveSelectedElements(xOffset, yOffset);
    }
}