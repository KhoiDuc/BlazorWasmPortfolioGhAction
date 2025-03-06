export class DotNetHelperWrapper {
    static dotNetHelper;

    static setDotNetHelper(value) {
        DotNetHelperWrapper.dotNetHelper = value;
    }

    static async callUpdateSelectedElementsInformation(informationLines) {
        await DotNetHelperWrapper.dotNetHelper.invokeMethodAsync
            ('UpdateSelectedElementsInformation', informationLines);
    }
}

let svgDiagram;
const svgDiagramGridStep = 20;

export function createSvgDiagram(svgId, width, height, shouldShowGrid, gridStep) {
    svgDiagram = new SvgDiagram(svgId, svgDiagramGridStep);
    svgDiagram.setParameters(width, height, shouldShowGrid, gridStep);

    svgDiagram.svg.node.addEventListener(
        SvgDiagramSelectionControls.selectedElementsInformationChangedEventName,
        async (event) => await DotNetHelperWrapper.callUpdateSelectedElementsInformation
            (event.detail.informationLines));
}

export function updateSvgDiagramParameters(width, height, shouldShowGrid, gridStep) {
    svgDiagram.setParameters(width, height, shouldShowGrid, gridStep);
}

export function addSvgDiagramRectangle() {
    svgDiagram.shapeGenerator.addRectangle();
}

export function addSvgDiagramCircle() {
    svgDiagram.shapeGenerator.addCircle();
}

export function addSvgDiagramLine() {
    svgDiagram.shapeGenerator.addLine();
}

export function removeSvgDiagramSelectedElements() {
    svgDiagram.removeSelectedElements();
}