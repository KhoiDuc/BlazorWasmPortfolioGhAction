class SvgDiagramSelectionControls {
    static selectionFrameClassName = "svg-diagram-selection-frame";
    static selectionFrameOffset = 4;
    static clickEventName = "click";
    static selectedElementsWillBeChangedEventName = "selectedElementsWillBeChanged";
    static selectedElementsChangedEventName = "selectedElementsChanged";
    static selectedElementsInformationChangedEventName = "selectedElementsInformationChanged";

    #svg;
    #selectionFrame;

    selectableElements = [];
    selectedElements = [];

    constructor(svg) {
        this.#svg = svg;
        let controls = this;
        this.#svg.on(SvgDiagramSelectionControls.clickEventName,
            (event) => SvgDiagramSelectionControls.onSvgClick(event, controls));
    }

    get selectionFrameIsShown() {
        return !!this.#svg.find(`.${SvgDiagramSelectionControls.selectionFrameClassName}`).length;
    }

    moveSelectedElements(xOffset, yOffset) {
        if (!this.selectedElements.length || (!xOffset && !yOffset)) {
            return;
        }

        for (let elementIndex = 0; elementIndex < this.selectedElements.length; elementIndex++) {
            let selectedElement = this.selectedElements[elementIndex];
            selectedElement.move(selectedElement.x() + xOffset, selectedElement.y() + yOffset);
        }

        this.#selectionFrame.move(this.#selectionFrame.x() + xOffset, this.#selectionFrame.y() + yOffset);

        this.dispatchSelectedElementsInformationChangedEvent();
    }

    getSelectedElementsInformationLines() {
        var result = [];
        if (!this.selectedElements.length) {
            return result;
        }

        for (let elementIndex = 0; elementIndex < this.selectedElements.length; elementIndex++) {
            let selectedElement = this.selectedElements[elementIndex];

            var informationLine = '';
            switch (selectedElement.type) {
                case 'rect':
                    informationLine = `Rectangle: x=${selectedElement.x()}, y=${selectedElement.y()}, `
                        + `width=${selectedElement.width()}, height=${selectedElement.height()}`;
                    break;
                case 'circle':
                    informationLine = `Circle: x=${selectedElement.x()}, y=${selectedElement.y()}, `
                        + `radius=${selectedElement.rx()}`;
                    break;
                case 'line':
                    var lineArray = selectedElement.array();
                    informationLine = `Line: x1=${lineArray[0][0]}, y1=${lineArray[0][1]}, `
                        + `x2=${lineArray[1][0]}, y2=${lineArray[1][1]}`;
                    break;
                default:
                    break;
            }

            if (informationLine) {
                result.push(informationLine);
            }
        }

        return result;
    }

    dispatchSelectedElementsInformationChangedEvent() {
        this.#svg.node.dispatchEvent(
            new CustomEvent(SvgDiagramSelectionControls.selectedElementsInformationChangedEventName, {
                detail: {
                    informationLines: this.getSelectedElementsInformationLines()
                }
            }),
        );
    }

    dispatchSelectedElementsWillBeChangedEvent() {
        this.#svg.node.dispatchEvent(
            new CustomEvent(SvgDiagramSelectionControls.selectedElementsWillBeChangedEventName, {
                detail: {
                    selectedElements: [...this.selectedElements]
                }}),
        );
    }

    dispatchSelectedElementsChangedEvent() {
        this.#svg.node.dispatchEvent(
            new CustomEvent(SvgDiagramSelectionControls.selectedElementsChangedEventName, {
                detail: {
                    selectedElements: this.selectedElements
                }
            }),
        );
    }

    recreateSelectionFrame() {
        this.removeSelectionFrame();

        if (!this.selectedElements.length) {
            return;
        }

        var rectangle = this.#getSelectedElementsRectangle();
        this.#selectionFrame = this.#svg.rect(rectangle.width, rectangle.height)
            .move(rectangle.x, rectangle.y)
            .addClass(SvgDiagramSelectionControls.selectionFrameClassName);
    }

    removeSelectionFrame() {
        if (this.selectionFrameIsShown) {
            this.#selectionFrame.remove();
        }
    }

    addSelectableElement(element) {
        let elementIsSelectable = this.selectableElements.indexOf(element) >= 0;
        if (elementIsSelectable) {
            return;
        }

        this.selectableElements.push(element);

        let controls = this;
        element.on(SvgDiagramSelectionControls.clickEventName,
            (event) => SvgDiagramSelectionControls.onSelectableElementClick(event, controls, element));
    }

    removeSelectableElement(element) {
        let selectableElementIndex = this.selectableElements.indexOf(element);
        let elementIsSelectable = selectableElementIndex >= 0;
        if (!elementIsSelectable) {
            return;
        }

        this.selectableElements.splice(selectableElementIndex, 1);

        let controls = this;
        element.off(SvgDiagramSelectionControls.clickEventName,
            (event) => SvgDiagramSelectionControls.onSelectableElementClick(event, controls, element));
    }

    removeSelectedElements() {
        if (!this.selectedElements.length) {
            return;
        }

        this.removeSelectionFrame();

        for (var elementIndex = this.selectedElements.length - 1;
            elementIndex >= 0; elementIndex--) {

            var element = this.selectedElements[elementIndex];
            this.removeSelectableElement(element);
            this.selectedElements.pop();
            element.remove();
        }

        this.dispatchSelectedElementsInformationChangedEvent();
    }

    #getSelectedElementsRectangle() {
        let result = new Rectangle();
        if (!this.selectedElements.length) {
            return result;
        }

        result.x = Number.MAX_SAFE_INTEGER;
        result.y = Number.MAX_SAFE_INTEGER;

        let right = 0;
        let bottom = 0;

        for (let elementIndex = 0; elementIndex < this.selectedElements.length; elementIndex++) {
            let element = this.selectedElements[elementIndex];

            let x = element.x();
            let y = element.y();

            result.x = Math.min(result.x, x);
            result.y = Math.min(result.y, y);

            right = Math.max(right, x + element.width());
            bottom = Math.max(bottom, y + element.height());
        }

        var offset = SvgDiagramSelectionControls.selectionFrameOffset;
        var doubleOffset = 2 * offset;

        result.width = right - result.x + doubleOffset;
        result.height = bottom - result.y + doubleOffset;

        result.x -= offset;
        result.y -= offset;

        return result;
    }

    static onSelectableElementClick(event, controls, element) {
        if (event.buttons != 0) {
            return;
        }

        event.stopPropagation();

        let selectedElementIndex = controls.selectedElements.indexOf(element);
        let elementIsSelected = selectedElementIndex >= 0;

        if (!event.ctrlKey) {
            if (elementIsSelected) {
                return;
            }
            controls.dispatchSelectedElementsWillBeChangedEvent();
            controls.selectedElements.splice(0, controls.selectedElements.length);
            controls.selectedElements.push(element);
        } else {
            controls.dispatchSelectedElementsWillBeChangedEvent();
            if (elementIsSelected) {
                controls.selectedElements.splice(selectedElementIndex, 1);
            } else {
                controls.selectedElements.push(element);
            }
        }

        controls.recreateSelectionFrame();
        controls.dispatchSelectedElementsChangedEvent();
        controls.dispatchSelectedElementsInformationChangedEvent();
    }

    static onSvgClick(event, controls) {
        if (event.buttons != 0 || !controls.selectedElements.length) {
            return;
        }

        controls.dispatchSelectedElementsWillBeChangedEvent();
        controls.selectedElements.splice(0, controls.selectedElements.length);
        controls.recreateSelectionFrame();
        controls.dispatchSelectedElementsChangedEvent();
        controls.dispatchSelectedElementsInformationChangedEvent();
    }
}