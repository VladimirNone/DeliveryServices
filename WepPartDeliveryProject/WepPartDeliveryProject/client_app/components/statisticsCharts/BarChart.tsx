import { ChartData } from "chart.js";
import { FC, useEffect, useState } from "react";
import { Bar } from 'react-chartjs-2';
import ChartDataLabels from 'chartjs-plugin-datalabels';

import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
  } from 'chart.js';
  
  ChartJS.register(
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend
  );

  function getRandomInt(max:number) {
    return Math.floor(Math.random() * max);
}

const BarChart: FC<{ query:statisticQueryInfo }> = ({ query }) => {
    const [chartData, setChartData] = useState<ChartData<"bar">>();

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/statistic/${query.linkToQuery}`, {
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                },
            });
            const resInfo = await resp.json() as statisticQueryDataItem[];
    
            const data:{labels:string[], datasets:any} = {
                labels: resInfo.map((value) => value.x),
                datasets: [],
            }
            
            for( let i = 0; i < (query.nameDatasets?.length ?? 1); i++)
            {
                data.datasets.push({
                    label: query.nameDatasets == null ? query.nameQuery : query.nameDatasets[i],
                    data: resInfo.map((value) => value.y[i]),
                    borderColor: `rgb(${getRandomInt(255)}, ${getRandomInt(255)}, ${getRandomInt(255)})`,
                    backgroundColor: `rgb(${getRandomInt(255)}, ${getRandomInt(255)}, ${getRandomInt(255)})`,
                    borderWidth: 1,
                });
            };

            setChartData(data);
        }
        fetchData();
    }, [query]);

    const options = {
        plugins: {
            datalabels: {
                color: '#000',
                font: {
                    weight: 'bold' as 'bold'
                },
                align: 'center' as 'center'
              }
        },
    };

    return (
        <>
            {chartData != undefined && <Bar plugins={[ChartDataLabels]} options={options} data={chartData}/>}
        </>    
    );
}

export default BarChart;