import React, { FC, ReactNode } from "react";

const Layers: FC<{ children: ReactNode }> = ({ children }) => {
  return (<div>
        {children}
    </div>);
};

export default Layers;
