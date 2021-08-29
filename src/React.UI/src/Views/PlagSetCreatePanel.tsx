import { FormItem, ObservableValue, Panel, React, Spinner, SpinnerOrientation, TextField } from "../AzureDevOpsUI";

export interface PlagSetCreatePanelProps {
  busy: boolean;
  valid: boolean;
  closeCreatePanel: () => void;
  sendCreateRequest: () => void;
  newPsetName: ObservableValue<string | undefined>;
  newPsetDescription: ObservableValue<string | undefined>;
}

export class PlagSetCreatePanel extends React.Component<PlagSetCreatePanelProps> {

  public render() {
    return (
      <Panel
          onDismiss={this.props.closeCreatePanel}
          titleProps={{ text: "Create Plagiarism Set" }}
          footerButtonProps={[
            { text: "Cancel", onClick: this.props.closeCreatePanel, disabled: this.props.busy },
            { text: "Create", onClick: this.props.sendCreateRequest, disabled: !this.props.valid || this.props.busy, primary: true }
          ]}>
        <form className="flex flex-column flex-grow">
          <FormItem label="Name" required>
            <TextField
              value={this.props.newPsetName}
              disabled={this.props.busy}
              onChange={(e, newValue) => (this.props.newPsetName.value = newValue)}
            />
          </FormItem>
          <FormItem label="Description">
            <TextField
              multiline
              rows={4}
              value={this.props.newPsetDescription}
              disabled={this.props.busy}
              onChange={(e, newValue) => (this.props.newPsetDescription.value = newValue)}
            />
          </FormItem>
          {this.props.busy &&
            <div style={{ margin: '24px auto 0 0' }} className="flex-row">
              <Spinner label="Creating new plagiarism set..." orientation={SpinnerOrientation.row} />
            </div>
          }
        </form>
      </Panel>
    );
  }
}
