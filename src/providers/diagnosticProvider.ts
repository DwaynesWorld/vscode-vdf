import * as vscode from "vscode";
import { VdfProxyService, getVdfProxyService } from "../client/vdfProxyService";
import { CommandType, ICommand, IDiagnosticResult } from "../client/proxy";

let diagnosticCollection: vscode.DiagnosticCollection;
let diagnosticProvider: DiagnosticProvider;

class DiagnosticProvider {
  constructor(private vdfProxyService: VdfProxyService) {
    diagnosticCollection = vscode.languages.createDiagnosticCollection("vdf");
  }

  provideDiagnostics(document: vscode.TextDocument) {
    const cmd: ICommand<IDiagnosticResult> = {
      command: CommandType.Diagnostics,
      fileName: document.fileName
    };

    this.vdfProxyService
      .getVdfProxyHandler<IDiagnosticResult>(document.uri)
      .sendCommand(cmd)
      .then(result => {
        console.log(result);

        if (result) {
          const diagnostics = result.diagnostics.map(diag => {
            return new vscode.Diagnostic(
              new vscode.Range(
                diag.range.startLine,
                diag.range.startColumn,
                diag.range.endLine,
                diag.range.endColumn
              ),
              diag.message,
              diag.serverity
            );
          });

          diagnosticCollection.clear();
          diagnosticCollection.set(document.uri, diagnostics);
        }
      });
  }
}

export function getDiagnosticsProvider() {
  if (diagnosticProvider == null) {
    const vdfProxyService = getVdfProxyService();
    diagnosticProvider = new DiagnosticProvider(vdfProxyService);
  }

  return diagnosticProvider;
}
