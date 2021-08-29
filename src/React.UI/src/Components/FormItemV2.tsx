import * as React from "react";
import { FormItem, IFormItemContext, IFormItemProps, FormItemContext } from "azure-devops-ui/FormItem";
import { css, getSafeId } from "azure-devops-ui/Util";
import "./FormItemV2.css";

export { FormItemContext };
export type { IFormItemContext };

export interface IFormItemV2Props extends IFormItemProps {
  required?: boolean;
  alternativeRequired?: boolean;
}

export class FormItemV2 extends React.Component<IFormItemV2Props> {

  public render() {
    return (
      <FormItem
          className={css(this.props.className, "bolt-formitem")}
          error={this.props.error}
          message={this.props.message}
      >
        {this.props.label && (
          <FormItemContext.Consumer>
            {value => (
              <label
                  aria-label={this.props.ariaLabel}
                  className={css(
                      "bolt-formitem-label",
                      "body-m",
                      this.props.required && "bolt-required",
                      this.props.alternativeRequired && "bolt-alternative-required"
                  )}
                  id={getSafeId(value.ariaLabelledById)}
              >
                {this.props.label}
              </label>
            )}
          </FormItemContext.Consumer>
        )}
        {this.props.children}
      </FormItem>
    );
  }
}
