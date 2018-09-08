import * as vscode from 'vscode';
import { CommandType, ICommand, IHoverResult } from '../client/proxy';
import { VdfProxyService } from '../client/vdfProxyService';

export class VdfHoverProvider implements vscode.HoverProvider {
  constructor(private vdfProxyService: VdfProxyService) {}

  provideHover(
    document: vscode.TextDocument,
    position: vscode.Position,
    token: vscode.CancellationToken
  ): Thenable<vscode.Hover> {
    // Comment line
    if (document.lineAt(position.line).text.match(/^\s*\/\//)) {
      return Promise.resolve(null);
    }

    // What the hell!
    if (position.character <= 0) {
      return Promise.resolve(null);
    }

    const fileName = document.fileName;
    const range = document.getWordRangeAtPosition(position);
    const possibleWord = document.getText(range);
    const command = CommandType.Definitions;
    const lineIndex = position.line;
    const columnIndex = range.isEmpty
      ? position.character
      : range.end.character;

    const cmd: ICommand<IHoverResult> = {
      command,
      possibleWord,
      fileName,
      columnIndex,
      lineIndex
    };

    var s: vscode.MarkdownString = new vscode.MarkdownString("#Test");
    var ss: vscode.MarkdownString = new vscode.MarkdownString(
      "##Testing More."
    );
    var sss: vscode.MarkdownString = new vscode.MarkdownString(
      "Actual Testing More."
    );

    var item: vscode.Hover = new vscode.Hover(s);
    item.contents.push(ss);
    item.contents.push(sss);

    return Promise.resolve(item);

    // return this.vdfProxyService
    //   .getVdfProxyHandler<IDefinitionResult>(document.uri)
    //   .sendCommand(cmd, token)
    //   .then(result => {
    //     if (result) {
    //       const locations = result.definitions.map(def => {
    //         const uri = vscode.Uri.file(def.filePath);
    //         return new vscode.Location(
    //           uri,
    //           new vscode.Range(
    //             def.range.startLine,
    //             def.range.startColumn,
    //             def.range.endLine,
    //             def.range.endColumn
    //           )
    //         );
    //       });
    //       if (locations.length > 0) return Promise.resolve(locations);
    //     }
    //     return Promise.reject(null);
    //   });

    return Promise.reject(null);
  }
}
