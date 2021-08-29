import * as React from "react";
import { IIconProps } from "azure-devops-ui/Icon";
import { Icon } from "@fluentui/react/lib/Icon";
import { Tooltip, ITooltipProps } from "azure-devops-ui/TooltipEx";
import { css } from "azure-devops-ui/Util";
import { Link } from "azure-devops-ui/Link";
import { Breadcrumb } from "azure-devops-ui/Breadcrumb";
import { RouteComponentProps } from "react-router";
import './ProjectHeader.css';

export class ProjectHeader extends React.Component<RouteComponentProps> {

  public render() {
    return (
      <div className="project-header flex-row flex-noshrink" role="navigation">
        <div className="flex-row flex-grow region-header" data-renderedregion="header" role="menubar">
          <Link title="Site Home Page" className="commandbar-item suite-logo flex-row flex-noshrink flex-center" href="/" role="menuitem" tabIndex={0}>
            <Icon iconName="VSTSLogo" className="suite-image commandbar-icon justify-center flex-noshrink compact-fabric-icon" />
          </Link>
          <div className="flex-row flex-grow scroll-hidden bolt-breadcrumb-with-items">
            <Breadcrumb
                className="header-breadcrumb flex-grow"
                items={[
                  {
                    key: "home",
                    text: "Home",
                    href: "/"
                  },
                  {
                    key: "plagiarism",
                    text: "Plagiarism Detection System",
                    href: "/"
                  }
                ]}
            />
          </div>
          <div aria-expanded="false" aria-haspopup="true" aria-label="My Work" className="commandbar-item commandbar-icon cursor-pointer flex-row flex-noshrink justify-center bolt-expandable-container flex-row flex-center" data-focuszone="focuszone-1" role="menuitem" tabIndex={-1}>
            <Icon iconName="CheckList" className="compact-fabric-icon medium" />
          </div>
          <div aria-expanded="false" aria-haspopup="true" aria-label="Marketplace" className="commandbar-item commandbar-icon cursor-pointer flex-row flex-noshrink justify-center bolt-expandable-container flex-row flex-center" data-focuszone="focuszone-1" role="menuitem" tabIndex={-1}>
            <Icon iconName="Shop" className="compact-fabric-icon medium" />
          </div>
          <div aria-expanded="false" aria-haspopup="true" aria-label="Help" className="commandbar-item commandbar-icon cursor-pointer flex-row flex-noshrink justify-center bolt-expandable-container flex-row flex-center" data-focuszone="focuszone-1" role="menuitem" tabIndex={-1}>
            <Icon iconName="Unknown" className="compact-fabric-icon medium" />
          </div>
          <div aria-expanded="false" aria-haspopup="true" aria-label="User settings" className="commandbar-item commandbar-icon cursor-pointer flex-row flex-noshrink justify-center bolt-expandable-container flex-row flex-center" data-focuszone="focuszone-1" role="menuitem" tabIndex={-1}>
            <Icon iconName="PlayerSettings" className="compact-fabric-icon medium" />
          </div>
        </div>
      </div>
    );
  }
}
