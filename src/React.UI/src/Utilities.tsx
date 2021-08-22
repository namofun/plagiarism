import * as React from "react";
import { IIconProps } from "azure-devops-ui/Icon";
import { Icon } from "@fluentui/react/lib/Icon";
import { Tooltip, ITooltipProps } from "azure-devops-ui/TooltipEx";
import { css } from "azure-devops-ui/Util";

interface IWithIconProps {
  className?: string;
  iconProps: IIconProps;
  children?: React.ReactNode;
  tooltipProps? : ITooltipProps;
}

export class WithIcon extends React.Component<IWithIconProps> {

  constructor(props: IWithIconProps) {
    super(props);
  }

  public render() {
    let content = (
      <div className={css(this.props.className, "flex-row flex-center")}>
        <Icon {...this.props.iconProps} className="icon-margin" />
        {this.props.children}
      </div>
    );

    if (this.props.tooltipProps) {
      content = (
        <Tooltip {...this.props.tooltipProps}>
          {content}
        </Tooltip>
      )
    }

    return content;
  }
}
